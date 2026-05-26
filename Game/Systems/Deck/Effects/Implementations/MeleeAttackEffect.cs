using Godot;
using System;
using System.Collections.Generic;
using System.Linq; // Needed for .FirstOrDefault()
using System.Text.Json; // NEW: Needed to handle JsonElement
using DeckroidVania.Game.Combat.Hitbox; // For AttackData
using DeckroidVania2.Game.Systems.Deck.CardEffects; // For CardMeleeEffect
using DeckroidVania2.Game.Systems.Deck.Effects; // For ICardEffect and EffectContext
using DeckroidVania2.Game.Player; // Needed if context.Source is cast to Player for IsFacingRight() etc.

// Make sure AttackLoader's namespace is included here if it's not global
// using Data; // Example if AttackLoader is in a global 'Data' namespace, or whatever its actual namespace is

namespace DeckroidVania2.Game.Systems.Deck.Effects.Implementations
{
    public partial class MeleeAttackEffect : ICardEffect
    {
        /// <summary>
        /// Executes the melee attack effect defined by a card.
        /// This method is called by the CardManager.
        /// </summary>
        /// <param name="context">The context of the effect (player node, target, tree).</param>
        /// <param name="parameters">A dictionary of parameters from the card's JSON activateEffects.</param>
        /// <returns>True if the effect was executed successfully, false otherwise.</returns>
        public bool Execute(EffectContext context, Dictionary<string, object> parameters)
        {
            GD.Print("[MeleeAttackEffect] Execute called.");

            // 1. Validate essential context and parameters
            if (context.Source == null || !(context.Source is Node3D playerNode))
            {
                GD.PrintErr("MeleeAttackEffect: Context Source is null or not a Node3D (Player). Effect aborted.");
                return false;
            }
            if (parameters == null)
            {
                GD.PrintErr("MeleeAttackEffect: Effect parameters are null. Effect aborted.");
                return false;
            }

            // 2. Extract parameters from the card's JSON (effectData.Params)
            //    MODIFIED: Now robustly handles JsonElement for numbers and strings.
            int attackId;
            if (!parameters.TryGetValue("attackId", out object attackIdObj) || !TryConvertToInt(attackIdObj, out attackId))
            {
                GD.PrintErr("MeleeAttackEffect: Missing or invalid 'attackId' parameter. Effect aborted.");
                return false;
            }

            string visualEffectScenePath;
            if (!parameters.TryGetValue("visualEffectScenePath", out object visualPathObj) || !TryConvertToString(visualPathObj, out visualEffectScenePath))
            {
                GD.PrintErr("MeleeAttackEffect: Missing or invalid 'visualEffectScenePath' parameter. Effect aborted.");
                return false;
            }

            string animationName;
            if (!parameters.TryGetValue("animationName", out object animNameObj) || !TryConvertToString(animNameObj, out animationName))
            {
                GD.PrintErr("MeleeAttackEffect: Missing or invalid 'animationName' parameter. Effect aborted.");
                return false;
            }

            string controllerScenePath;
            if (!parameters.TryGetValue("controllerScenePath", out object controllerPathObj) || !TryConvertToString(controllerPathObj, out controllerScenePath))
            {
                GD.PrintErr("MeleeAttackEffect: Missing or invalid 'controllerScenePath' parameter. Effect aborted.");
                return false;
            }

            // 3. Load the specific AttackData from attacks.json using the attackId.
            List<AttackData> allAttacks = AttackLoader.LoadAttacksFromJson("Data/attacks.json"); // Assume this path is correct
            AttackData currentAttackData = allAttacks?.FirstOrDefault(a => a.Id == attackId);

            if (currentAttackData == null)
            {
                GD.PrintErr($"MeleeAttackEffect: Could not find AttackData for attackId: {attackId}. Effect aborted.");
                return false;
            }

            // 4. Load the CardMeleeEffect controller scene (our visual and hitbox orchestrator).
            PackedScene cardMeleeEffectPackedScene = GD.Load<PackedScene>(controllerScenePath);

            if (cardMeleeEffectPackedScene == null)
            {
                GD.PrintErr($"MeleeAttackEffect: Failed to load CardMeleeEffect scene from '{controllerScenePath}'. Effect aborted.");
                return false;
            }
            CardMeleeEffect cardMeleeEffectInstance = cardMeleeEffectPackedScene.Instantiate<CardMeleeEffect>();

            if (cardMeleeEffectInstance == null)
            {
                GD.PrintErr($"MeleeAttackEffect: Instantiated scene from '{controllerScenePath}' is not of type CardMeleeEffect. Effect aborted.");
                return false;
            }

            // 5. Add the instantiated CardMeleeEffect to the scene tree.
            playerNode.AddChild(cardMeleeEffectInstance);

            // 6. Call StartAttack() on the instantiated CardMeleeEffect.
            cardMeleeEffectInstance.StartAttack(currentAttackData, visualEffectScenePath, playerNode, animationName);

            GD.Print($"[MeleeAttackEffect] Successfully launched CardMeleeEffect for attack '{currentAttackData.Name}' (ID: {attackId}) with visual '{visualEffectScenePath}' and animation '{animationName}' using controller '{controllerScenePath}'.");
            return true;
        }

        // --- NEW: Helper methods for robust parameter conversion ---
        private bool TryConvertToInt(object obj, out int result)
        {
            if (obj is int intVal)
            {
                result = intVal;
                return true;
            }
            if (obj is JsonElement element && element.ValueKind == JsonValueKind.Number)
            {
                return element.TryGetInt32(out result);
            }
            result = default;
            return false;
        }

        private bool TryConvertToString(object obj, out string result)
        {
            if (obj is string stringVal)
            {
                result = stringVal;
                return true;
            }
            if (obj is JsonElement element && element.ValueKind == JsonValueKind.String)
            {
                result = element.GetString();
                return result != null;
            }
            result = default;
            return false;
        }
    }
}