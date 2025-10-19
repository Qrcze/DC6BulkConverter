### DC6 Bulk Converter
Simple console program for decoding .dc6 files (Diablo II images/animation format), either in bulk or one-by-one, and converting them into viewable image formats.

Requires .NET 8 to run.

### Usage examples
```DC6BulkConverter.exe input_folder out```
Will convert all of the images from the `input_folder` into `out` folder (creating it, if necessary), each image either as .png image if it has single frame, or as .gif if it has multiple frames.

```DC6BulkConverter.exe input_folder out -f png```
Will convert all of the images into .png image, adding suffixes: "_frame#" to the images with multiple frames.

#### Links with explanations for .dc6 format
 - https://d2mods.info/forum/viewtopic.php?t=724#p148076
 - https://gist.github.com/MarkKoz/874052801d7eddd1bb4a9b69cd1e9ac8
 
 