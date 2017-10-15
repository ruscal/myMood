//
//  AVFrameDecoder.h
//
//  Created by Moses DeJong on 12/30/10.
//
//  License terms defined in License.txt.
//
//  This abstract superclass defines the interface that needs to be implemented by
//  a class that implements frame decoding logic. A frame is 2D image decoded from
//  video data from a file or other resource.

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@class CGFrameBuffer;

@interface AVFrameDecoder : NSObject {
}

// Open resource identified by path

- (BOOL) openForReading:(NSString*)path;

// Close resource opened earlier

- (void) close;

// Reset current frame index to -1, before the first frame

- (void) rewind;

// Advance the current frame index to the indicated frame index. Return the new frame
// encoded as a UIImage object, or nil if the frame data was not changed. The UIImage
// returned is assumed to be in the autorelease pool.

- (UIImage*) advanceToFrame:(NSUInteger)newFrameIndex;

// Decoding frames may require additional resources that are not required
// to open the file and examine the header contents. This method will
// allocate decoding resources that are required to actually decode the
// video frames from a specific file. It is possible that allocation
// could fail, for example if decoding would require too much memory.
// The caller would need to check for a FALSE return value to determine
// how to handle the case where allocation of decode resources fails.

- (BOOL) allocateDecodeResources;

- (void) releaseDecodeResources;

// Returns TRUE if resources are "limited" meaning decode resource are not allocated.

- (BOOL) isResourceUsageLimit;

// Return a copy of the last frame returned via advanceToFrame.
// This copy will not be associated with the frame decoder and
// it will not contain any external references to shared memory.
// This method is useful only for the case where holding onto a
// ref to the final frame will waste significant resources, for
// example if the normal frames hold references to mapped memory.

- (UIImage*) duplicateCurrentFrame;

// Properties

// Dimensions of each frame
- (NSUInteger) width;
- (NSUInteger) height;

// True when resource has been opened via openForReading
- (BOOL) isOpen;

// Total frame count
- (NSUInteger) numFrames;

// Current frame index, can be -1 at init or after rewind
- (NSInteger) frameIndex;

// Time each frame shold be displayed
- (NSTimeInterval) frameDuration;

// TRUE if the decoded frame supports and alpha channel.
- (BOOL) hasAlphaChannel;

@end
