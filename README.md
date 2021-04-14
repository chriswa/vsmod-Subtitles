# vsmod-Subtitles

Adds directional subtitles for sound effects.

## Caveats

This mod doesn't take the volume of the sound sample into consideration (only the volume and distance reported in the game engine.)

This mod doesn't take into consideration that some sounds can block out other sounds (for example, a heavy rainstorm can make quieter sounds impossible to hear.) This could cause gameplay balance issues.

There is currently no way to reposition the subtitles. Pull requests welcome!

## Work In Progress

The sound-to-english translations provided in this mod are incomplete. If you'd like to contribute better translations, it's quite easy to edit the `en.json` file, so please provide a pull request.

Note that two sounds with the same "translation" result will be merged into one "line". Also, a sound with a translation to an empty string (`""`) will not be shown at all.
