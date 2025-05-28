# XR Laser Tag System

A mixed reality laser tag system combining physical laser hardware with immersive XR visualization using Unity and the PICO 4 headset.

## System Overview

This project demonstrates integration of:

- **Physical Hardware**: Laser gun with AprilTag targets
- **Computer Vision**: C# application using OpenCV for detection
- **XR Visualization**: Unity-based HUD with real-time feedback
- **Mixed Reality**: PICO 4 headset with passthrough capability

## Repository Structure

```
├── Unity/
│   ├── Scripts/           # Unity C# scripts
│   ├── Prefabs/          # Visual effect prefabs
│   └── Scenes/           # Unity scene files
├── Detection/            # C# OpenCV application
└── Documentation/        # Technical documentation
```

## Unity Scripts

### Core Game Logic

#### `ScoreManager.cs`

**Purpose**: Central scoring and statistics system

- Tracks player score, shots fired, and accuracy
- Manages game timer and session state
- Updates UI elements
- Provides game reset functionality

**Key Methods**:

- `AddShot(bool hit, int scoreValue)` - Records shot attempt and updates score
- `ResetGame()` - Resets all statistics and timer
- `UpdateUI()` - Refreshes all UI text displays

#### `ResetTag.cs`

**Purpose**: Special target that resets the game when hit

- Communicates with ScoreManager to reset game state
- Triggers visual/audio effects on interaction
- Provides clean game restart mechanism

**Usage**: Attach to GameObjects that should reset the game when hit by laser

### Visual & Audio Feedback

#### `TagHitEffect.cs`

**Purpose**: Handles visual and audio feedback for all target interactions (audio currently in seperate project and not in Unity)

- Instantiates particle effects at hit locations
- Plays spatial audio clips for immersive feedback
- Supports different effects for normal hits vs. reset actions

**Configuration**:

- `hitEffectPrefab` - Particle effect for normal hits
- `resetEffectPrefab` - Particle effect for reset interactions
- `hitSound` - Audio clip for normal hits
- `resetSound` - Audio clip for reset actions

#### `ScoreboardAnimator.cs`

**Purpose**: Provides dynamic visual feedback for the HUD interface

- Pulse animation for score changes
- Glow effects for text highlighting
- Smooth coroutine-based animations

**Features**:

- Configurable pulse intensity and duration
- Text outline color animation
- Automatic animation cleanup

### XR Integration

#### `EnableSeeThrough.cs`

**Purpose**: PICO 4 passthrough integration (not fit for production use as no direct camera access)

- Configures camera for mixed reality rendering
- Enables PICO 4 video passthrough functionality

**Status**: Currently commented out for PC demonstration setup

## Quick Start

### Prerequisites

- Unity 2022.3+ with PICO Unity Integration SDK
- PICO 4 headset
- Physical laser hardware setup
- AprilTag targets

### Setup Instructions

1. Import Unity project
2. Configure PICO 4 XR settings
3. Assign script references in inspector
4. Configure UI prefabs and audio clips
5. Build for PICO 4 or use PC mirroring for demo

## Development Notes

### Current Implementation

- Unity HUD system with real-time statistics
- Visual effects and spatial audio feedback
- Prepared for full PICO 4 integration
- Demonstration setup using PC mirroring

### Future Enhancements

- Native PICO 4 camera integration
- Advanced particle effects and haptic feedback

## Academic Context

Developed for "Industrial Games in XR" course, demonstrating:

- Mixed reality development with Unity
- XR hardware integration (PICO 4)
- Computer vision application in gaming
- Modular software architecture
- Performance optimization for VR

## Usage Example

```csharp
// Basic usage pattern for integrating with detection system
public class LaserDetectionBridge : MonoBehaviour
{
    public ScoreManager scoreManager;
    public TagHitEffect hitEffect;

    // Called when laser hit detected by external system
    public void OnLaserHit(Vector3 hitPosition, bool isTarget)
    {
        if (isTarget)
        {
            scoreManager.AddShot(true, 10);
            hitEffect.PlayHitEffect(hitPosition);
        }
        else
        {
            scoreManager.AddShot(false);
        }
    }
}
```
