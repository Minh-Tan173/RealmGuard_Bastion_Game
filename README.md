# 1: Realmguard Bastion
Realmguard Bastion is a 2D Tower Defense game developed using the Unity Engine.
The project was developed with the goal of researching and implementing key gameplay systems in tower defense games, such as:
* Tower Combat System
* Ability Support System
* Progression & Unlock System
* Procedural Map Generation
  
> The entire project was **developed solo** with the goal of creating **a clean and easily scalable gameplay architecture**.

# 2: Gameplay Overview
In Realmguard Bastion, players must build defensive structures to stop waves of incoming enemies.
Each level follows a core gameplay loop:

1. Build Towers
2. Upgrade Towers
3. Defend against Enemy Waves
4. Use Abilities To support the defense
5. Complete waves to earn rewards

> Players must **strategically place towers, manage resources, and use abilities effectively** to protect their base.

# 3: Technical Highlights
## Procedural Grid-Based Map Generation
- Implemented **a multi-stage procedural map generation pipeline** that builds the map using a grid-based architecture.
- The generation process follows a structured pipeline:
  * Background → Ground → Path → Object Placement
- This system ensures that generated maps always maintain **valid enemy paths** and **compatible tower placement areas**.

## Grid-Based Gameplay Architecture
- The core gameplay systems are built on top of a GridNode-based grid structure. Each node stores map-related data such as position, tile type, and object placement state.
- This grid is used by multiple systems including:
  * Map generation
  * Tower placement
  * Path generation
  * Environment Object placement

## Event-Driven System Communication
- Several gameplay systems communicate through C# events, reducing tight coupling between modules.
- This approach is used across different gameplay layers, including:
  * Map generation stages
  * Enemy wave events
  * Gameplay interactions
## Object Pooling for Enemy Spawning
- Implemented **an object pooling system** in the enemy spawning logic to reuse enemy instances instead of repeatedly instantiating and destroying them.
- This helps:
  * Reduce runtime allocations
  * Improve performance during large enemy waves
## ScriptableObject Data-Driven Design  
- Many gameplay systems are built using **ScriptableObject**-based data configuration.
- This allows flexible tuning of gameplay elements such as:
  * Enemy Data
  * Ability configuration
  * Level setup
  * Environment assets
without modifying core code

## Modular Tower Architecture
- All towers inherit from a shared **BaseTower class**, allowing common functionality while enabling unique mechanics for each tower type.
- Current tower implementations include:
  * Archer Tower
  * Mage Tower
  * Guardian Tower
  * Catapult Tower
  * 
## Wave-Based Enemy System
Enemy spawning is organized using a wave-based system that manages enemy groups, spawn timing, and wave progression throughout each level.

## Singleton-Based Core Managers
Several core gameplay managers are implemented using the **Singleton pattern to provide** centralized access and maintain consistent global state across systems.

# 4: Tower System
## Archer Tower
- The Archer Tower is the most basic and versatile defensive structure.
- Design goals:
  * Provide **stable** and **reliable damage**
  * Handle multiple enemy types
  * Serve as the **foundation of early-game defense**
- Key characteristics:
  *  Archer has **75° vision**
  *  Can attack **all target**
  *  Balanced attack speed and damage
> Because of its flexibility, Archer Towers are commonly used to **cover multiple lanes and provide consistent damage output** throughout the game.

## Mage Tower
- The Mage Tower focuses on **high attack speed** and **scaling damage**.
- Design goals:
  * Counter fast-moving enemies
  * Scale damage significantly through upgrades
  * Handle dense enemy waves
- Key characteristics:
  * Mage has **90° vision**
  * Low base damage
  * High attack speed
  * Magic projectiles that pierce through multiple enemies
> This tower excels at punishing enemies that rely on speed rather than durability.

## Guardian Tower
The Guardian Tower operates by **summoning Guardian units that physically block enemies**, rather than attacking from range like other towers.
- Design goals:
  * Intercept specific ground enemies capable of dealing physical damage
  * Establish a frontline defensive layer
  * Protect other towers positioned behind it
-  Key characteristics:
  * Summons Guardian units that engage enemies in direct combat
  * Guardians can only block certain ground enemies that possess physical attack capabilities
  * Guardians fight enemies in direct melee confrontation
  * Guardians receive a damage buff only while actively engaging an enemy face-to-face
> Guardian Tower controls enemy movement and forms a frontline defense, slowing enemies down, protecting damage-dealing towers behind it, and enabling other towers to deal damage more effectively.

## Catapult Tower
- The Catapult Tower specializes in long-range area damage.
- Design goals:
  * Destroy large enemy groups
  * Provide high burst damage
  * Support late-game wave clearing
- Key characteristics:
  * Long attack range
  * **AoE damage**
  * Projectile volleys that can hit multiple enemies
> Catapult Towers are particularly effective when enemy waves become larger and more densely packed.

# 5: Ability System
> Abilities enhance the defense system by providing diverse powers and unique ways to deal damage.

The available abilities include:
- **Spike Trap**: A trap placed on the path that damages enemies when stepped on.
- **Thunder Lightning**: Summons instant lightning strikes at the selected location.
- **Explosive Barrel**: A barrel that explodes and deals large area-of-effect damage.

# 6: World Map & Progression System
- The game uses a World Map to manage player progression.
- From the World Map, players can:
  * Select campaign levels
  * View tower information
  * View ability information
  * Unlock new content
- After completing a level, players receive Blue Flame Points.
- These points can be used to:
  * Unlock new abilities
  * Unlock higher tower upgrade levels (this feature will be added in a future update)
    
# 7: Procedural Map Generation System
## Progress: Background → Ground → Path → Object Placement
- One of the key technical systems in the project is **the Procedural Map Generation System**.
- The system is built on a grid-based procedural generation approach, allowing dynamic map layouts while maintaining gameplay constraints required for tower defense mechanics.
- The goals of this system are:
  * Generate varied map layouts
  * Ensure valid enemy paths
  * Support tower placement logic
  * Enable future custom map generation
## Grid Node Structure
- The map is represented using a GridNode structure.
- Each node stores information such as:
  * Position
  * Grid type
  * Index
  * Object placement state
- This grid acts as the **core data structure for the entire map generation pipeline

## Background Generation
- The first step of the map generation process is Background Generation.
- **The BackgroundGenerator** is responsible for creating the base layer of the map by:
  * Generating background tiles
  * Filling the area that visually supports the playable map
> This layer serves as the visual foundation of the environment.

## Ground Generation
- After the background is created, the GroundGenerator builds the main terrain of the map based on the grid system.
- The system will:
  * Initialize the grid map
  * Generate **floor tiles** where towers can be placed
  * Create **fence tiles** along the map borders to define map boundaries
> The ground layer becomes the primary gameplay area where players interact with the tower placement system.

## Path Generation
- Once the ground map is created, the PathGenerator generates the enemy path.
- The process includes:
  1. Determine **Spawn Point**
  2. Determine **Base Destination**
  3. Generate a continuous path between the two points
- The algorithm ensures:
  * The path is continuous
  * No dead-ends paths are created
  * Enemies always have a valid traversal path
> This guarantees that the map is always playable from a tower defense perspective.

## Object Placement
- After the terrain and path are completed, the ObjectPlaced system handles environmental object placement.
- The system will:
  * Spawn environmental objects across the map
  * Avoid placing objects on enemy paths
  * Distribute objects according to the terrain layout
> This step helps make the map feel more natural and visually dynamic.

# 8: Code Architecture
The project is structured using a modular gameplay architecture to improve scalability and maintainability.

```
Core Systems
├── LevelManager
├── GameInput
├── WaveSystem
└── EconomySystem

Tower System
├── BaseTower
├── ArcherTower
├── MageTower
├── GuardianTower
└── CatapultTower

Enemy System
├── BaseEnemy
├── EnemyMovement
└── EnemyCombat

Ability System
├── BaseAbility
├── SpikeTrap
├── ExplosiveBarrel
└── ThunderLightning

Map Generation
├── GridNode
├── PathGenerator
├── GroundGenerator
├── BackgroundGenerator
└── ObjectPlaced
```
- This structure allows:
  * Easy addition of new towers
  * Expansion of the ability system
  * Future implementation of custom map generation

## 9: Future Development Plans
Planned improvements include:
- Advanced Tower Upgrades: Towers will be able to upgrade to levels 5–7, which must be unlocked through the World Map progression system using Blue Flame Points.
- Custom Mode:
- **A Custom Mode** will use the procedural generation system to automatically generate:
  * Maps
  * Enemy waves
  * Gameplay scenarios
- This system will rely on: LevelManagerSO, WaveScript

## Author:
- Game Developer / System Architect: Minh Tan
  * Email: minhtanjjjj@gmail.com
  * Portfolio: 
