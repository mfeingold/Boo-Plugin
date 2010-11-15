Here is what you need to build the Boo: Visual Studio extension:

1. Visual Studio 2010

2. Visual Studio 2010 SDK - you can download it [[here|http://www.microsoft.com/downloads/en/details.aspx?FamilyID=47305cf4-2bea-43c0-91cd-1b853602dcc5&displaylang=en]]

3. The code from this repository. This code includes the solution with the extension projects. The Microsoft.VisualStudio.Project project from the solution is a fork from [[Managed Package Framework for Projects|http://mpfproj10.codeplex.com/]] rev 55578. The BooProject project also has a set of boo files from boo distribution (in the boo_files subdirectory)

To run the extension set the BooProject as startup project and run the solution. 
It will open a new instance of VisualStudio running in Experimental Hive, meaning that all registry/file changes necessary 
to make the extension available are made in a separate set of registry entries/directories and can be easily rolled back 
without affecting your working instance of Visual Studio.

To make available the version you compiled in the main hive, find the BooPlugin.vsix file in the bin folder of the BooProject and run it.
