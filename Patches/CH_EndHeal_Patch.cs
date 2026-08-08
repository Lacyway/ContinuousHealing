using System.Reflection;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace ContinuousHealing.Patches;

internal sealed class CH_EndHeal_Patch : ModulePatch
{
    public static int Animation;
    public static bool CancelRequested;

    protected override MethodBase GetTargetMethod()
    {
        return typeof(Player.MedsController.MedsInHandsOperation)
            .GetMethod("OnEffectRemoved");
    }

    [PatchPrefix]
    public static bool Prefix(Player.MedsController.MedsInHandsOperation __instance, IHealthEffect effect)
    {
        if (CancelRequested)
        {
            __instance.ClearQueue();
            __instance.Finish();
            return true;
        }

#if DEBUG
        CH_Plugin.CH_Logger.LogWarning($"Effect is: {effect.GetType()}, Item is: {__instance.IHealthEffect.Item.GetType()}]");
#endif
        if (effect is not IMedEffect)
        {
#if DEBUG
            CH_Plugin.CH_Logger.LogWarning("Was not a MedEffect! Ignoring...");
#endif
            return false;
        }

#if DEBUG
        if (effect is ActiveHealthController.GClass3008 durEffect)
        {
            CH_Plugin.CH_Logger.LogWarning("It's a durEffect, delay: " + durEffect.DelayTime);
        }
#endif

        var player = __instance._controller._player;
        if (player == null)
        {
            return true;
        }

        if (!player.IsYourPlayer)
        {
            return true;
        }

        if (__instance._controller.Item is not Meds && (!CH_Plugin.HealLimbs.Value || __instance._controller.Item is not Medical))
        {
#if DEBUG
            CH_Plugin.CH_Logger.LogWarning($"Item was not of MedKitItemClass/MedicalItemClass type, was: {__instance.IHealthEffect.Item.GetType()}");
#endif
            return true;
        }

        var medsItem = (Meds)__instance._controller.Item;
        if (medsItem == null)
        {
            CH_Plugin.CH_Logger.LogError("medsItem was null!");
            return true;
        }

        if (medsItem.MedKitComponent == null)
        {
#if DEBUG
            CH_Plugin.CH_Logger.LogWarning("MedKitComponent was null! Probably a single-use...");
#endif
            return true;
        }

        if (medsItem.MedKitComponent.HpResource <= 1 && medsItem.MedKitComponent.MaxHpResource < 95)
        {
#if DEBUG
            CH_Plugin.CH_Logger.LogWarning("Resource was equalTo or lessThan 1 and not a healing kit, skipping...");
#endif
            return true;
        }

        if (player.ActiveHealthController.CanApplyItem(__instance._controller.Item, EBodyPart.Common))
        {
#if DEBUG
            CH_Plugin.CH_Logger.LogWarning("Can apply again!");
#endif
            player.HealthController.EffectRemovedEvent -= __instance.OnEffectRemoved;
            var originalDelay = ActiveHealthController._settings.Effects.MedEffect.MedKitStartDelay;
            ActiveHealthController._settings.Effects.MedEffect.MedKitStartDelay = (float)CH_Plugin.HealDelay.Value;
            var newEffect = player.ActiveHealthController.DoMedEffect(__instance._controller.Item, EBodyPart.Common, 1f);
            if (newEffect == null)
            {
                __instance.State = Player.EOperationState.Finished;
                __instance._controller.FailedToApply = true;
                var callbackToRun = __instance._onUsedCallback;
                __instance._onUsedCallback = null;
                callbackToRun(__instance._controller);
                ActiveHealthController._settings.Effects.MedEffect.MedKitStartDelay = originalDelay;
                return false;
            }
            ;
            player.HealthController.EffectRemovedEvent += __instance.OnEffectRemoved;
            ActiveHealthController._settings.Effects.MedEffect.MedKitStartDelay = originalDelay;

            if (CH_Plugin.ResetAnimation.Value && __instance._controller.Item is not Meds)
            {
                Animation++;
                var variant = 0;
                if (__instance._controller.Item.TryGetItemComponent(out AnimationVariantsComponent animationVariantsComponent))
                {
                    variant = animationVariantsComponent.VariantsNumber;
                }

                var newAnim = (int)Mathf.Repeat((float)Animation, (float)variant);
#if DEBUG
                CH_Plugin.CH_Logger.LogWarning($"New anim: {newAnim}");
#endif

                if (__instance._controller.FirearmsAnimator != null)
                {
                    var mult = player.Skills.SurgerySpeed.Value / 100f;
                    var animator = __instance._controller.FirearmsAnimator;
                    
                    animator.SetUseTimeMultiplier(1f + mult);
                    if (animator.HasNextLimb())
                    {
#if DEBUG
                        CH_Plugin.CH_Logger.LogWarning("Has next limb!");
#endif
                        animator.SetNextLimb(true);
                        animator.SetActiveParam(false, false);
                    }
#if DEBUG
                    CH_Plugin.CH_Logger.LogWarning("Setting new anim");
#endif
                    animator.SetAnimationVariant(newAnim);
                }
            }

            return false;
        }

        return true;
    }
}
