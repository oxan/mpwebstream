This is the default FFmpeg as shipped by MPWebStream. It is an extracted version of ffmpeg-git-a304071-win32-static.7z, as available on http://hawkeye.arrozcru.org/. It contains ffmpeg revision a3040715e1f0db1af0c27566a306c4a27ad07dcd, available on http://git.videolan.org/?p=ffmpeg.git. All credits go to the FFmpeg developers for creating the software (see http://ffmpeg.org/) and Kyle Schwarz for compiling it for windows (see http://hawkeye.arrozcru.org/).

This is a static 32-bit build, which should run on all computers. If you have a 64-bit CPU and Windows, you can use a 64-bit ffmpeg, which has slightly better performance. Switching to shared builds is also possible. 

You can freely replace this ffmpeg by another version, as long as the command line interface remains the same (which is true for 99,9% of all ffmpeg versions). Deleting it is also possible, but then the default transcoding configuration doesn't work anymore.

