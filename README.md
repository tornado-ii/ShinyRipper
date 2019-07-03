# EversionRipper
A two-input command line program that rips graphics from Eversion (Classic) and Eversion HD (Bundle + Steam).

This program is a Windows command line program that utilises a GZip library to unpack and read data from Eversion (Classic) and Eversion HD (Bundle + Steam).
The required libraries are packaged in the executable and do not require extra downloads. This functionality is made possible thanks to [adriancs](https://www.codeproject.com/articles/528178/load-dll-from-embedded-resource).
This program was created with the assistance of shrubbyfrog and is dedicated to Su "Moth" Tolias.

# How to use EversionRipper
1. After downloading or building the application, open your *Eversion* game folder.
2. Navigate to the folder containing the sprites you want to rip.
   1. If you want to rip character sprites or tile sprites, head to `Eversion/chrs/`. 
   2. If you want to rip backgrounds and title graphics, head to `Eversion/images/`.
3. Either...
   1. Drag the archive (`.cha` or `.zrs`) to *EversionRipper.exe*
   2. Open *EversionRipper.exe* and drag the archive (`.cha` or `.zrs`) to the console window when asking for a path
   3. Open *EversionRipper.exe* and manually type in the path to the archive (`.cha` or `.zrs`)
4. Press enter/return to submit the path to the program.
5. The program will now ask if you would like to "convert transparent pixels". *Eversion* sprites use a default colour (either specified in the header of the extracted archive or found in `Eversion/scripts/main.rb`) to represent transparency. Selecting 'yes' will make these transparent instead of the solid colour. Selecting 'no' will output the sprite with this background colour. The program will only accept four inputs at this point:
   1. Y (yes, convert background colour to transparent pixels)
   2. N (no, don't convert background colour to transparent pixels)
   3. X or Escape (quit application)
6. The program will go through the extracted archive and render all the sprites to `.png` format in a folder in the same directory as the original archive. *(e.g. `Eversion/chrs/player/`)*.
   - If a folder or file without extension with the same name already exists, the application will refuse to render the sprites due to naming conflicts (I don't want to overwrite your files). You will need the remove the conflicting file or folder and try again.
7. The program will exit automatically with the message `OK!` indicating it has gone through the entirety of the ripping function. Your sprites should now be located in a folder in the same directory as the archive was in. The sprites will be named after the archive with an appended digit indicating its index in the archive.
