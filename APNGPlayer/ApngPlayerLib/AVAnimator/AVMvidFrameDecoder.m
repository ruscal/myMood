//
//  AVMvidFrameDecoder.m
//  QTFileParserApp
//
//  Created by Moses DeJong on 4/23/11.
//  Copyright 2011 __MyCompanyName__. All rights reserved.
//

#import "AVMvidFrameDecoder.h"

#import "CGFrameBuffer.h"

#import "maxvid_file.h"

#ifndef __OPTIMIZE__
// Automatically define EXTRA_CHECKS when not optimizing (in debug mode)
# define EXTRA_CHECKS
#endif // DEBUG

//#define ALWAYS_CHECK_ADLER

@implementation AVMvidFrameDecoder

@synthesize filePath = m_filePath;
@synthesize mappedData = m_mappedData;
@synthesize currentFrameBuffer = m_currentFrameBuffer;
@synthesize cgFrameBuffers = m_cgFrameBuffers;


#if defined(REGRESSION_TESTS)
@synthesize simulateMemoryMapFailure = m_simulateMemoryMapFailure;
#endif // REGRESSION_TESTS

- (void) dealloc
{
  [self close];

  self.filePath = nil;
  self.mappedData = nil;
  self.currentFrameBuffer = nil;
  
  /*
   for (CGFrameBuffer *aBuffer in self.cgFrameBuffers) {
   int count = [aBuffer retainCount];
   count = count;
   
   if (aBuffer.isLockedByDataProvider) {
   NSString *msg = [NSString stringWithFormat:@"%@, count %d",
   @"CGFrameBuffer is still locked by UIKit", count];
   NSLog(msg);
   
   if ([aBuffer isLockedByImageRef:imgRef1]) {
   NSLog(@"locked by imgRef1");
   } else if ([aBuffer isLockedByImageRef:imgRef2]) {
   NSLog(@"locked by imgRef2");
   } else if ([aBuffer isLockedByImageRef:imgRef3]) {
   NSLog(@"locked by imgRef3");
   } else {
   NSLog(@"locked by unknown image ref");				
   }
   }
   }
   */

  self.cgFrameBuffers = nil;
  
	[super dealloc];
}

- (id) init
{
	if ((self = [super init]) != nil) {
    self->frameIndex = -1;
    self->m_resourceUsageLimit = TRUE;
  }
	return self;
}

+ (AVMvidFrameDecoder*) aVMvidFrameDecoder
{
  return [[[AVMvidFrameDecoder alloc] init] autorelease];
}

- (MVFileHeader*) _getHeader
{
  NSAssert(self->m_isOpen == TRUE, @"isOpen");
  return &self->m_mvHeader;
}

- (void) _allocFrameBuffers
{
	// create buffers used for loading image data
  
  if (self.cgFrameBuffers != nil) {
    // Already allocated the frame buffers
    return;
  }
  
	int renderWidth = [self width];
	int renderHeight = [self height];
  
  NSAssert(renderWidth > 0 && renderHeight > 0, @"renderWidth or renderHeight is zero");

  NSAssert(m_mvFile, @"m_mvFile");
  uint32_t bitsPerPixel = [self _getHeader]->bpp;
  
	CGFrameBuffer *cgFrameBuffer1 = [CGFrameBuffer cGFrameBufferWithBppDimensions:bitsPerPixel width:renderWidth height:renderHeight];
  CGFrameBuffer *cgFrameBuffer2 = [CGFrameBuffer cGFrameBufferWithBppDimensions:bitsPerPixel width:renderWidth height:renderHeight];
  CGFrameBuffer *cgFrameBuffer3 = [CGFrameBuffer cGFrameBufferWithBppDimensions:bitsPerPixel width:renderWidth height:renderHeight];
  
	self.cgFrameBuffers = [NSArray arrayWithObjects:cgFrameBuffer1, cgFrameBuffer2, cgFrameBuffer3, nil];
  
  // Double check size assumptions
  
  if (bitsPerPixel == 16) {
    NSAssert(cgFrameBuffer1.bytesPerPixel == 2, @"invalid bytesPerPixel");
  } else if (bitsPerPixel == 24 || bitsPerPixel == 32) {
    NSAssert(cgFrameBuffer1.bytesPerPixel == 4, @"invalid bytesPerPixel");
  } else {
    NSAssert(FALSE, @"invalid bitsPerPixel");
  }
  
  self->m_resourceUsageLimit = FALSE;
}

- (void) _freeFrameBuffers
{
  self.currentFrameBuffer = nil;
  self.cgFrameBuffers = nil;
}

- (CGFrameBuffer*) _getNextFramebuffer
{
	[self _allocFrameBuffers];
  
	CGFrameBuffer *cgFrameBuffer = nil;
	for (CGFrameBuffer *aBuffer in self.cgFrameBuffers) {
    if (aBuffer == self.currentFrameBuffer) {
      // When a framebuffer is the "current" one, it contains
      // the decoded output from a previous frame. Need to
      // ignore it and select the next available one.
      continue;
    }    
		if (!aBuffer.isLockedByDataProvider) {
			cgFrameBuffer = aBuffer;
			break;
		}
	}
	if (cgFrameBuffer == nil) {
		NSAssert(FALSE, @"no cgFrameBuffer is available");
	}
  return cgFrameBuffer;
}

// This utility method will read the header data from an mvid
// file without mapping it into memory. The contents of the
// header will be copied so that header metadata can be
// queried without having to map the whole file. A mvid file
// can't be changed after the header is written, so it is safe
// to cache the contents of the header and then map and unmap
// the whole file as needed without worry of an invalid cache.

- (BOOL) _openAndCopyHeader
{
  MVFileHeader *hPtr = &self->m_mvHeader;

  char* filenameCstr = (char*)[self.filePath UTF8String];
  FILE *fp = fopen(filenameCstr, "rb");
  if (fp == NULL) {
    // Return FALSE to indicate that the file could not be opened
    return FALSE;
  }
  
  BOOL worked = TRUE;
  
  // Copy the whole header into the struct. Any valid file will
  // contain a whole header, so if the read does not work then
  // the file is not valid.
  
  assert(sizeof(MVFileHeader) == 16*4);
  assert(sizeof(MVFrame) == 3*4);
  
  int numRead = fread(hPtr, sizeof(MVFileHeader), 1, fp);
  if (numRead != 1) {
    // Could not read header from file, it must be empty or invalid
    worked = FALSE;
  }
  
  
  if (worked) {
    uint32_t magic = hPtr->magic;
    if (magic != MV_FILE_MAGIC) {
      // Reading the header worked, but if the magic number is not valid then
      // this is not a valid maxvid file. Could have been another kind of file
      // or could have been a partially written maxvid file.
      
      worked = FALSE;
    }
    
    if (worked) {
      uint32_t bpp = hPtr->bpp;
      NSAssert(bpp == 16 || bpp == 24 || bpp == 32, @"bpp must be 16, 24, 32");
    }
  }
  
  fclose(fp);
  return worked;
}

// Private utils to map the .mvid file into memory.
// Return TRUE if memory map was successful or file is already mapped.
// Otherwise, returns FALSE when memory map was not successful.

- (BOOL) _mapFile {
  if (self.mappedData == nil) {
    // Might need to map a very large mvid file in terms of 24 Meg chunks,
    // would want to write it that way?

    BOOL memoryMapFailed = FALSE;

#if defined(REGRESSION_TESTS)
    if (self.simulateMemoryMapFailure) {
      memoryMapFailed = TRUE;
    }
#endif // REGRESSION_TESTS

    if (memoryMapFailed == FALSE) {    
      self.mappedData = [NSData dataWithContentsOfMappedFile:self.filePath];
      if (self.mappedData == nil) {
        memoryMapFailed = TRUE;
      }
    }
    
    if (memoryMapFailed == TRUE) {
      return FALSE;
    }
    
    self->m_resourceUsageLimit = FALSE;
    void *mappedPtr = (void*)[self.mappedData bytes];
    self->m_mvFile = maxvid_file_map_open(mappedPtr);
  }
  
  return TRUE;
}

- (void) _unmapFile {
  if (m_mvFile != NULL) {
    maxvid_file_map_close(m_mvFile);
    self->m_mvFile = NULL;
  }  
  self.mappedData = nil;
}

- (BOOL) openForReading:(NSString*)moviePath
{
	if (self->m_isOpen) {
		return FALSE;
  }
  
  if (![[moviePath pathExtension] isEqualToString:@"mvid"]) {
    return FALSE;
  }
  
  self.filePath = moviePath;
  
  // Opening the file will verify that the header is correct to ensure that the file was not
  // partially written. The header will then be read so that this object has access to
  // the data contained in the header. Note that the file need not be successfully mapped
  // into memory at this point. It is possible that many files could be open but the file
  // need not be mapped into memory until it is actually used.  
  
  BOOL worked = [self _openAndCopyHeader];
  if (!worked) {
    self.filePath = nil;
    return FALSE;
  }

	self->m_isOpen = TRUE;
	return TRUE;
}

// Close resource opened earlier

- (void) close
{
  [self _unmapFile];
  
	frameIndex = -1;
  self.currentFrameBuffer = nil;
  
  self->m_isOpen = FALSE;  
}

- (void) rewind
{
	if (!self->m_isOpen) {
		return;
  }
  
	frameIndex = -1;
  self.currentFrameBuffer = nil;
}

- (UIImage*) advanceToFrame:(NSUInteger)newFrameIndex
{
  // The movie data must have been mapped into memory by the time advanceToFrame is invoked
  
  NSAssert(self.mappedData, @"file not mapped");
  
  // Get from queue of frame buffers!
  
  CGFrameBuffer *nextFrameBuffer = [self _getNextFramebuffer];
  
  // Double check that the current frame is not the exact same object as the one we pass as
  // the next frame buffer. This should not happen, and we can't copy the buffer into itself.
  
  NSAssert(nextFrameBuffer != self.currentFrameBuffer, @"current and next frame buffers can't be the same object");  
  
  // Advance to same frame is a no-op
  
	if ((frameIndex != -1) && (newFrameIndex == frameIndex)) {
    return nil;
	} else if ((frameIndex != -1) && (newFrameIndex < frameIndex)) {
    // movie frame index can only go forward via advanceToFrame
		NSString *msg = [NSString stringWithFormat:@"%@: %d -> %d",
                     @"can't advance to frame before current frameIndex",
                     frameIndex,
                     newFrameIndex];
		NSAssert(FALSE, msg);
  }
  
	// Get the number of frames directly from the header
	// instead of invoking method to query self.numFrames.
  
  int numFrames = [self numFrames];
  
	if (newFrameIndex >= numFrames) {
		NSString *msg = [NSString stringWithFormat:@"%@: %d",
                     @"can't advance past last frame",
                     newFrameIndex];
		NSAssert(FALSE, msg);
	}
  
	BOOL changeFrameData = FALSE;
	const int newFrameIndexSigned = (int) newFrameIndex;
  
  char *mappedPtr = (char*) [self.mappedData bytes];
  NSAssert(mappedPtr, @"mappedPtr");
  
  void *frameBuffer = (void*)nextFrameBuffer.pixels;
  uint32_t frameBufferSize = [self width] * [self height];
  uint32_t bpp = [self _getHeader]->bpp;
  uint32_t frameBufferNumBytes;
  if (bpp == 16) {
    frameBufferNumBytes = frameBufferSize * sizeof(uint16_t);
  } else {
    frameBufferNumBytes = frameBufferSize * sizeof(uint32_t);
  }
  
  // Check for the case where multiple frames need to be processed,
  // if one of the frames between the current frame and the target
  // frame is a keyframe, then save time by skipping directly to
  // that keyframe (avoids memcpy when not needed) and then
  // applying deltas from the keyframe to the target frame.
  
  if ((newFrameIndexSigned > 0) && ((newFrameIndexSigned - frameIndex) > 1)) {
    int lastKeyframeIndex = -1;
    
    for ( int i = frameIndex ; i < newFrameIndexSigned; i++) {
      int actualFrameIndex = i + 1;
      MVFrame *frame = maxvid_file_frame(self->m_mvFile, actualFrameIndex);
      
      if (maxvid_frame_isnopframe(frame)) {
        // This frame is a no-op, since it duplicates data from the previous frame.
      } else {
        if (maxvid_frame_iskeyframe(frame)) {
          lastKeyframeIndex = i;
        }
      }
    }
    // Don't set frameIndex for the first frame (frameIndex == -1)
    if (lastKeyframeIndex > -1) {
      frameIndex = lastKeyframeIndex;
      
#ifdef EXTRA_CHECKS
      int actualFrameIndex = frameIndex + 1;
      MVFrame *frame = maxvid_file_frame(self->m_mvFile, actualFrameIndex);
      NSAssert(maxvid_frame_iskeyframe(frame) == 1, @"frame must be a keyframe");
#endif // EXTRA_CHECKS      
    }
  }
  
  // loop from current frame to target frame, applying deltas as we go.
  
	for ( ; frameIndex < newFrameIndexSigned; frameIndex++) {
    int actualFrameIndex = frameIndex + 1;
    MVFrame *frame = maxvid_file_frame(self->m_mvFile, actualFrameIndex);

#ifdef EXTRA_CHECKS
    if (actualFrameIndex == 0) {
      // First frame must be a keyframe
      NSAssert(maxvid_frame_iskeyframe(frame) == 1, @"initial frame must be a keyframe");
    }
#endif // EXTRA_CHECKS
    
    if (maxvid_frame_isnopframe(frame)) {
      // This frame is a no-op, since it duplicates data from the previous frame.
      //      fprintf(stdout, "Frame %d NOP\n", actualFrameIndex);
    } else {
      //      fprintf(stdout, "Frame %d [Size %d Offset %d Keyframe %d]\n", actualFrameIndex, frame->offset, movsample_length(frame), movsample_iskeyframe(frame));
			changeFrameData = TRUE;
      
      if (self.currentFrameBuffer != nextFrameBuffer) {
        // Copy the previous frame buffer unless there was not one, or current is a keyframe
        
        if (self.currentFrameBuffer != nil && !maxvid_frame_iskeyframe(frame)) {
          [nextFrameBuffer copyPixels:self.currentFrameBuffer];
        }
        self.currentFrameBuffer = nextFrameBuffer;
      } else {
        // In the case where the current cgframebuffer contains is a zero copy pointer, need to
        // explicitly copy the data from the zero copy buffer to the framebuffer.
        if (self.currentFrameBuffer.zeroCopyPixels != NULL) {
          [self.currentFrameBuffer zeroCopyToPixels];
        }
      }
      
      uint32_t status;
      
      uint32_t *inputBuffer32 = (uint32_t*) (mappedPtr + maxvid_frame_offset(frame));
      uint32_t inputBuffer32NumBytes = maxvid_frame_length(frame);

      if (maxvid_frame_iskeyframe(frame)) {
#ifdef EXTRA_CHECKS
        // FIXME: use zero copy of pointer into mapped file, impl OS page copy in util class
        if (bpp == 16) {
          NSAssert(inputBuffer32NumBytes == (frameBufferSize * sizeof(uint16_t)), @"framebuffer num bytes");
        } else {
          NSAssert(inputBuffer32NumBytes == (frameBufferSize * sizeof(uint32_t)), @"framebuffer num bytes");
        }
        NSAssert(((uint32_t)inputBuffer32 % MV_PAGESIZE) == 0, @"framebuffer num bytes");
#endif // EXTRA_CHECKS
    
#if defined(EXTRA_CHECKS) || defined(ALWAYS_CHECK_ADLER)
        // If mvid file has adler checksum for frame, verify that it matches
        
        if (frame->adler != 0) {
          uint32_t frameAdler = maxvid_adler32(0, (unsigned char*)inputBuffer32, inputBuffer32NumBytes);
          NSAssert(frame->adler == frameAdler, @"frameAdler");
        }        
#endif // EXTRA_CHECKS
  
        [nextFrameBuffer zeroCopyPixels:inputBuffer32 mappedData:self.mappedData];
      } else {
#ifdef EXTRA_CHECKS
        NSAssert(((uint32_t)inputBuffer32 % sizeof(uint32_t)) == 0, @"inputBuffer32 alignment");
        NSAssert((inputBuffer32NumBytes % sizeof(uint32_t)) == 0, @"inputBuffer32NumBytes");
#endif // EXTRA_CHECKS        
        uint32_t inputBuffer32NumWords = inputBuffer32NumBytes >> 2;
        if (bpp == 16) {
          status = maxvid_decode_c4_sample16(frameBuffer, inputBuffer32, inputBuffer32NumWords, frameBufferSize);
        } else {
          status = maxvid_decode_c4_sample32(frameBuffer, inputBuffer32, inputBuffer32NumWords, frameBufferSize);
        }
        NSAssert(status == 0, @"status");
        
#if defined(EXTRA_CHECKS) || defined(ALWAYS_CHECK_ADLER)
        // If mvid file has adler checksum for frame, verify that it matches the decoded framebuffer contents    
        if (frame->adler != 0) {
          uint32_t frameAdler = maxvid_adler32(0, (unsigned char*)frameBuffer, frameBufferNumBytes);
          NSAssert(frame->adler == frameAdler, @"frameAdler");
        }        
#endif // EXTRA_CHECKS
      }
    }
	}
  
  if (!changeFrameData) {
    return nil;
  } else {
    // Return a CGImage wrapped in a UIImage
    
    CGFrameBuffer *cgFrameBuffer = self.currentFrameBuffer;
    CGImageRef imgRef = [cgFrameBuffer createCGImageRef];
    NSAssert(imgRef, @"CGImageRef returned by createCGImageRef is NULL");
    
    UIImage *uiImage = [UIImage imageWithCGImage:imgRef];
    CGImageRelease(imgRef);
    
    NSAssert(cgFrameBuffer.isLockedByDataProvider, @"image buffer should be locked by frame UIImage");
    
    NSAssert(uiImage, @"uiImage is nil");
    return uiImage;    
  }
}

- (UIImage*) duplicateCurrentFrame
{
  if (self.currentFrameBuffer == nil) {
    return nil;
  }
  
  // Create an in-memory copy of the current frame buffer and return a new image wrapped around the copy
  
  CGFrameBuffer *cgFrameBuffer = [CGFrameBuffer cGFrameBufferWithBppDimensions:self.currentFrameBuffer.bitsPerPixel
                                                                         width:self.currentFrameBuffer.width
                                                                        height:self.currentFrameBuffer.height];
  
  // Using the OS level copy means that a small portion of the mapped memory will stay around, only the copied part.
  // Might be more efficient, unknown.
  
  //[cgFrameBuffer copyPixels:self.currentFrameBuffer];
  [cgFrameBuffer memcopyPixels:self.currentFrameBuffer];
  
  CGImageRef imgRef = [cgFrameBuffer createCGImageRef];
  NSAssert(imgRef, @"CGImageRef returned by createCGImageRef is NULL");
  
  UIImage *uiImage = [UIImage imageWithCGImage:imgRef];
  CGImageRelease(imgRef);
  
  NSAssert(cgFrameBuffer.isLockedByDataProvider, @"image buffer should be locked by frame UIImage");
  
  NSAssert(uiImage, @"uiImage is nil");
  return uiImage;  
}

- (void) resourceUsageLimit:(BOOL)enabled
{
  self->m_resourceUsageLimit = enabled;  
}

- (BOOL) allocateDecodeResources
{
  NSAssert(self->m_isOpen == TRUE, @"isOpen");
  
  [self resourceUsageLimit:FALSE];
  
  // FIXME: should this logic also allocate input buffers and frame buffers?
  
  BOOL worked = [self _mapFile];
  if (!worked) {
    return FALSE;
  }
  return TRUE;
}

- (void) releaseDecodeResources
{
  [self resourceUsageLimit:TRUE];
  
  [self _freeFrameBuffers];
  [self _unmapFile];
}

- (BOOL) isResourceUsageLimit
{
  return self->m_resourceUsageLimit;
}

// Return the current frame buffer, this is the buffer that was most recently written to via
// a call to advanceToFrame. Returns nil on init or after a rewind operation.

- (CGFrameBuffer*) currentFrameBuffer
{
  return self->m_currentFrameBuffer;
}

// Properties

- (NSUInteger) width
{
  return [self _getHeader]->width;
}

- (NSUInteger) height
{
  return [self _getHeader]->height;
}

- (BOOL) isOpen
{
  return self->m_isOpen;
}

- (NSUInteger) numFrames
{
  return [self _getHeader]->numFrames;
}

- (int) frameIndex
{
  // FIXME: What is the initial value of frameIndex, seems to be zero in MV impl, is it -1 in MOV reader?
  
  return self->frameIndex;
}

- (NSTimeInterval) frameDuration
{
  float frameDuration = [self _getHeader]->frameDuration;
  return frameDuration;
}

// Note that the file need to be open in order to query the bpp, but it need not be mapped.

- (BOOL) hasAlphaChannel
{
  uint32_t bpp = [self _getHeader]->bpp;
  if (bpp == 16 || bpp == 24) {
    return FALSE;
  } else if (bpp == 32) {
    return TRUE;
  } else {
    assert(0);
  }
}

@end
