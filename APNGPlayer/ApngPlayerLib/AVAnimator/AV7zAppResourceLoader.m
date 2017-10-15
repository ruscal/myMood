//
//  AV7zAppResourceLoader.m
//
//  Created by Moses DeJong on 4/22/11.
//
//  License terms defined in License.txt.
//

#import "AV7zAppResourceLoader.h"

#import "AVFileUtil.h"

#import "LZMAExtractor.h"

//#define LOGGING

@implementation AV7zAppResourceLoader

@synthesize archiveFilename = m_archiveFilename;
@synthesize outPath = m_outPath;

- (void) dealloc
{
  self.archiveFilename = nil;
  self.outPath = nil;
  [super dealloc];
}

+ (AV7zAppResourceLoader*) aV7zAppResourceLoader
{
  return [[[AV7zAppResourceLoader alloc] init] autorelease];
}

// This method is invoked in the secondary thread to decode the contents of the archive entry
// and write it to an output file (typically in the tmp dir).

+ (void) decodeThreadEntryPoint:(NSArray*)arr {  
  NSAutoreleasePool *pool = [[NSAutoreleasePool alloc] init];
  
  NSAssert([arr count] == 4, @"arr count");
  
  // Pass ARCHIVE_PATH ARCHIVE_ENTRY_NAME TMP_PATH
  
  NSString *archivePath = [arr objectAtIndex:0];
  NSString *archiveEntry = [arr objectAtIndex:1];
  NSString *phonyOutPath = [arr objectAtIndex:2];
  NSString *outPath = [arr objectAtIndex:3];

#ifdef LOGGING
  NSLog(@"start 7zip extraction %@", archiveEntry);
#endif // LOGGING  
  
  BOOL worked;
  worked = [LZMAExtractor extractArchiveEntry:archivePath archiveEntry:archiveEntry outPath:phonyOutPath];
  assert(worked);
  
#ifdef LOGGING
  NSLog(@"done 7zip extraction %@", archiveEntry);
#endif // LOGGING  
  
  // Move phony tmp filename to the expected filename once writes are complete
  
  [AVFileUtil renameFile:phonyOutPath toPath:outPath];
  
  [pool drain];
}

- (void) _detachNewThread:(NSString*)archivePath
            archiveEntry:(NSString*)archiveEntry
            phonyOutPath:(NSString*)phonyOutPath
                 outPath:(NSString*)outPath
{
  NSArray *arr = [NSArray arrayWithObjects:archivePath, archiveEntry, phonyOutPath, outPath, nil];
  NSAssert([arr count] == 4, @"arr count");
  
  [NSThread detachNewThreadSelector:@selector(decodeThreadEntryPoint:) toTarget:self.class withObject:arr];  
}

- (void) load
{
  // Avoid kicking off mutliple sync load operations. This method should only
  // be invoked from a main thread callback, so there should not be any chance
  // of a race condition involving multiple invocations of this load mehtod.
  
  if (startedLoading) {
    return;
  } else {
    self->startedLoading = TRUE;    
  }

  // Superclass load method asserts that self.movieFilename is not nil
  [super load];

  NSString *qualPath = [AVFileUtil getQualifiedFilenameOrResource:self.archiveFilename];
  NSAssert(qualPath, @"qualPath");

  NSString *archiveEntry = self.movieFilename;
  NSString *outPath = self.outPath;
  NSAssert(outPath, @"outPath not defined");

  // Generate phony tmp path that data will be written to as it is extracted.
  // This avoids thread race conditions and partial writes. Note that the filename is
  // generated in this method, and this method should only be invoked from the main thread.

  NSString *phonyOutPath = [AVFileUtil generateUniqueTmpPath];

  [self _detachNewThread:qualPath archiveEntry:archiveEntry phonyOutPath:phonyOutPath outPath:outPath];
  
  return;
}

// Output movie filename must be redefined

- (NSString*) _getMoviePath
{
 return self.outPath;
}

@end
