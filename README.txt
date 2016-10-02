#ArcBeat

Fairly basic two-player cooperative music game. Made as a combination class-assignment and personal excercise in synchronizing code execution to an audio clip running in parallel, which is rather more complicated than I initially expected and thus became the most interesting challenge of the project.

Videos:
https://www.youtube.com/watch?v=vy2sFBIVGGk
https://www.youtube.com/watch?v=hK-E4PPJZTQ
https://www.youtube.com/watch?v=O8zM6vm0F4s

Gameplay:

First, run the game starting in the Config scene. Select whether or not each side should be on auto-play (this must be done before selecting a file - the checkboxes become unclickable after, for unknown reason). Click the choose-a-file button. Navigate through the browser to a .osu file (from the music game Osu) and select it. The audio file matched to that .osu file must be in .ogg or .wav formats due to Unity restrictions, unless you're willing to pay the relevant licenses for other formats and integrate them accordingly; externally converting the audio from any other Osu! beatmap will work fine. When you select, the browser will disappear; after it has loaded, the original buttons will reappear - it won't take long, not more than a few seconds. Hit the Go button to transition to the game.

The left side uses WASD, the right side uses arrow keys. Press the directions as the falling arrows arrive, they will either be bounced up or disappear. If you hold the modifier key (defaults to left-shift for the left player, right-Ctrl for the right player), bounces will stay on your side. If you don't, bounces will go to the other side. At the top, scores are kept: hits, misses, and hits/total, for each player and combined.
