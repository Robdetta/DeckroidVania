# Enemy System ECS Refactoring Roadmap

## Current State Analysis

### What We Have:

- ✅ HealthComponent (already exists, mostly good)
- ❌ EnemyMovementController (doing too much - movement + AI + combat)
- ❌ AttackState (mixed concerns)
- ✅ EnemyAnimationTree (needs wrapper)
- ❌ Enemy.cs (orchestration unclear)

### What We Need:

- ✅ IHealthComponent interface
- ✅ IMovementComponent interface
- ✅ ICombatComponent interface
- ✅ IAIComponent interface
- ✅ IAnimationComponent interface

---

## Phase 2: Create Component Implementations

### 2.1 HealthComponent (Wrap Existing)

**File**: `Game/Entities/Enemies/Components/HealthComponent.cs`

- Wrap existing HealthComponent to implement IHealthComponent
- Already mostly done, just needs interface

### 2.2 MovementComponent (Extract from EnemyMovementController)

**File**: `Game/Entities/Enemies/Components/MovementComponent.cs`

- Extract ONLY movement logic from EnemyMovementController:
  - Velocity management
  - Horizontal/vertical movement
  - Wall/edge detection (raycasts)
  - Facing direction
  - MoveAndSlide execution

### 2.3 CombatComponent (NEW - Extract from EnemyMovementController + AttackState)

**File**: `Game/Entities/Enemies/Components/CombatComponent.cs`

- Attack selection based on distance
- Attack rotation/cycling
- Cooldown management
- ExecuteAttack() triggers animation

### 2.4 AIComponent (Extract State Machine from EnemyMovementController)

**File**: `Game/Entities/Enemies/Components/AIComponent.cs`

- State machine (Idle, Patrol, Chase, Attack, Knockback)
- State transitions
- Target tracking
- Decision making
- Delegates to other components for execution

### 2.5 AnimationComponent (Wrap EnemyAnimationTree)

**File**: `Game/Entities/Enemies/Components/AnimationComponent.cs`

- Wrapper around EnemyAnimationTree
- Implements IAnimationComponent
- Exposes clean animation API

---

## Phase 3: Refactor Enemy Base Class

### 3.1 Enemy.cs Becomes Orchestrator

```csharp
public partial class Enemy : CharacterBody3D
{
    // Components
    protected IHealthComponent HealthComponent;
    protected IMovementComponent MovementComponent;
    protected ICombatComponent CombatComponent;
    protected IAIComponent AIComponent;
    protected IAnimationComponent AnimationComponent;

    // Enemy just coordinates components!
    public override void _Ready()
    {
        InitializeComponents();
    }

    public override void _PhysicsProcess(double delta)
    {
        AIComponent.Update(delta);
        CombatComponent.Update(delta);
        MovementComponent.ApplyMovement(delta);
    }
}
```

---

## Phase 4: Update Enemy Types

### 4.1 MageEnemy.cs

- Loads mage.json
- Creates components with mage-specific data
- Passes AttackRanges to CombatComponent
- Sets AI to "AttackOnDetect" behavior

### 4.2 KnightEnemy.cs

- Loads knight.json
- Creates components with knight-specific data
- Passes AttackRanges to CombatComponent
- Sets AI to "ChaseOnDetect" behavior

---

## Phase 5: Delete/Archive Old Files

Once refactored, DELETE:

- ❌ EnemyMovementController.cs (logic extracted)
- ❌ AttackState.cs (logic in CombatComponent)
- ❌ IdleState.cs (logic in AIComponent)
- ❌ PatrolState.cs (logic in AIComponent)
- ❌ ChaseState.cs (logic in AIComponent)
- ❌ FallingState.cs (logic in AIComponent)
- ❌ KnockBackState.cs (logic in AIComponent)

KEEP:

- ✅ Enemy.cs (refactored as orchestrator)
- ✅ EnemyAnimationTree.cs (wrapped by AnimationComponent)
- ✅ MageAnimationTree.cs (wrapped by AnimationComponent)
- ✅ AttackDatabase.cs (shared service)
- ✅ EnemyData.cs (data classes)

---

## Benefits of This Approach

### ✅ Separation of Concerns

Each component has ONE job and does it well.

### ✅ Testability

Can test CombatComponent without needing full Enemy setup.

### ✅ Reusability

MovementComponent can work with any entity, not just enemies.

### ✅ Maintainability

Bug in combat? Look at CombatComponent. Bug in movement? Look at MovementComponent.

### ✅ Extensibility

Want flying enemies? Swap MovementComponent. Want boss AI? Swap AIComponent.

---

## Next Steps

1. **Create MovementComponent** (extract from EnemyMovementController)
2. **Create CombatComponent** (extract from AttackState + EnemyMovementController)
3. **Create AIComponent** (extract state machine from EnemyMovementController)
4. **Create AnimationComponent** (wrap EnemyAnimationTree)
5. **Refactor Enemy.cs** to use components
6. **Update MageEnemy/KnightEnemy** to initialize components
7. **Test thoroughly**
8. **Delete old files**

---

## Timeline Estimate

- Phase 2: ~2-3 hours (create components)
- Phase 3: ~1 hour (refactor Enemy.cs)
- Phase 4: ~30 minutes (update enemy types)
- Phase 5: ~15 minutes (cleanup)
- **Total: ~4 hours of focused work**

---

## Risk Mitigation

Before starting:

1. **Commit current code** to git
2. **Create a new branch** for refactoring
3. **Test after each phase**
4. **Keep old code commented** until new system works

Ready to proceed?
