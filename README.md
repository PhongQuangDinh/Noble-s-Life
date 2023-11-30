# Noble's life
## A castle
- Including 4 walls
    - Visual: 4 sprites
    - Physical: 4 Mountain lines
        - Re-use built-ins mountain...
- Castle total health: the main base health, only collapse base health meaning 
    - Composite structure
- Castle gate health
    - The isolated part of the gate, if the gate health is downn the gate no longer close
    - The defenders can repair the castle 
- Attackers with range weapon cannot shoot people under the castle wall
- Saving and loading castle sprites and tile system
- Closing gate by placing the mountain tile at the front of the gate 
## City mechanic
- Each City only have 1 castle if meet the required condition
    - hook into game event
- City buil-in defend mechanic, when regconize enemeies on the border of the city there will be evaluation for the commander of that army to raise alarm or not
## Attackers
- Attackers with melee weapon will focus on castle gate

# LICENSE
https://github.com/PhongQuangDinh/Noble-s-Life/blob/77cf5548d11398a71cdeb5c81c150800101abc2e/LICENSE.md