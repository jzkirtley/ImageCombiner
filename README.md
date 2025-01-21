ImageCombiner is a simple program designed to combine multiple image files in a single folder into a composite image file that can be read by Tabletop Simulator to create deck files.

Usage:
ImageCombiner (-RowSize <row-size>) (-MaxRows <max-rows>) (-MarginX <margin-x>) (-MarginY <margin-y>) SourceFolder (TargetFolder)

RowSize is the number of cards on a row. Defaults to 10.
MaxRows is the maximum number of rows that will be created. Default is 5.
Margin_X and Margin_Y are Horizontal and Vertical Margins in the cards. Default Values are 0. These values are in pixels, so some experimentation will be necessary.

if TargetFolder is omitted, the Parent folder of the Source Folder will be used.

ImageCombiner will create multiple files if there are more files in the folder than RowSize * MaxRows.

If necessary, ImageCombiner will scale the resulting image down to fix within the limitations of Tabletop Simulator.
