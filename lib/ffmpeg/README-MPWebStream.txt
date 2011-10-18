This is the default FFmpeg as shipped by MPWebStream. It is an extracted version of ffmpeg-git-6bca574-win32-static.7z, as available on http://ffmpeg.zeranoe.com/builds/. It contains ffmpeg revision 6bca574a98ba604459f6ee92538b19d25bba0973, with source code available on http://git.videolan.org/?p=ffmpeg.git and http://ffmpeg.zeranoe.com/. All credits go to the FFmpeg developers for creating the software (see http://ffmpeg.org/) and Kyle Schwarz for compiling it for windows (see http://ffmpeg.zeranoe.com/).

This is a static 32-bit build, which should run on all computers. If you have a 64-bit CPU and Windows, you can use a 64-bit ffmpeg, which has slightly better performance. Switching to shared builds is also possible. 

You can freely replace this ffmpeg by another version, as long as the command line interface remains the same (which is true for 99,9% of all ffmpeg versions). Deleting it is also possible, but then the default transcoding configuration doesn't work anymore.

