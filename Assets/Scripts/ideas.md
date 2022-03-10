## IDEAS & THOUGHTS

### Difficulty - parameters
</br>

#### Ghost zones
- Add more / less tiles with 'ghost zones' complicating ghost's movement
  - **options**:
    - ghosts movement speed decreases
    - ghosts are not allowed to move in a certain direction
  - **con** ... this needs special visual affordances:
    - this is expected to complicate the gameplay
    - changes in such visual affordances could influence the overal brightness

</br>


#### Speed:
  - **speed ratio** - ghost : pacman
  - **overall gameplay speed**


</br>

#### Map variation

| Option                | Personal experience & thoughts                     |
|---|---|
| extra **teleports**       | clearly easier!  <br> _(only horizontal currently)_|
| extra **energizers**      | feels saver - more relaxed                         |
| highly connected **maze** |  more difficult & easier at the same time <br>_(less easy to hight, but ghost reach Pacman more quickly)_  |
| **open spaces**           | not able to notice a big diference (yet)      |


</br>

#### Pathfinding strategy ghosts
Currently implemented pathfinding options:

|Pathfinding setting | enum / tag     | description                            |
|---|---|---|
| Blinky             | TARGET_PACMAN  | target pacman's tile                   |         
| Inky               | COLLABORATE    | 2 times the vector from blinky to 2 tiles in front Pacman|
| Pinky              | AHEAD_OF_PACMAN| offset 4 tiles away from Pac-Man in PM's direction |
| Clyde              | CIRCLE_AROUND  | if(distanceToPacman) > 8 ? pacman's curile : scatterTile |
| Frightened         | random         | random movement based on random generator |
| Scatter state      | scatter        | scatter tile is targeted               |


</br></br></br></br>
___
</br>

### OTHER

#### Data tracking
- see online google doc
</br></br>

</br>

####  Max's Pacman
##### vs original pacman:
- no smooth movement
- frightened - scattertile targeting instead of random
- ghosts @home - no internal ghost timer, only a global timer with standard settings - no timer after stopping eating dots
- reverse direction - immediately change direction vs. first finish current move to next tile
- not slowed down in tunnel?
</br>

##### Other relevant things that stand out
- hardcoded speed
