using System.Reflection;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace ContinuousHealing.Patches;

internal class CH_EndHeal_Patch : ModulePatch
{
    private static FieldInfo _playerField;

    public static int Animation;
    public static bool CancelRequested;

    protected override MethodBase GetTargetMethod()
    {
        _playerField = AccessTools.Field(typeof(Player.MedsController), "_player");
        return typeof(Player.MedsController.ObservedMedsControllerClass)
            .GetMethod("method_8");
    }

    [PatchPrefix]
    public static bool Prefix(Player.MedsController.ObservedMedsControllerClass __instance, IEffect effect)
    {
        if (CancelRequested)
        {
            __instance.ClearQueue();
            __instance.method_9();
            return true;
        }

#if DEBUG
        CH_Plugin.CH_Logger.LogWarning($"Effect is: {effect.GetType()}, Item is: {__instance.MedsController_0.Item.GetType()}]");
#endif
        if (effect is not GInterface376)
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

        var player = (Player)_playerField.GetValue(__instance.MedsController_0);
        if (player == null)
        {
            return true;
        }

        if (!player.IsYourPlayer)
        {
            return true;
        }

        if (__instance.MedsController_0.Item is not MedKitItemClass && (!CH_Plugin.HealLimbs.Value || __instance.MedsController_0.Item is not MedicalItemClass))
        {
#if DEBUG
            CH_Plugin.CH_Logger.LogWarning($"Item was not of MedKitItemClass/MedicalItemClass type, was: {__instance.MedsController_0.Item.GetType()}");
#endif
            return true;
        }

        var medsItem = (MedsItemClass)__instance.MedsController_0.Item;
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

        if (player.ActiveHealthController.CanApplyItem(__instance.MedsController_0.Item, EBodyPart.Common))
        {
#if DEBUG
            CH_Plugin.CH_Logger.LogWarning("Can apply again!");
#endif
            player.HealthController.EffectRemovedEvent -= __instance.method_8;
            var originalDelay = ActiveHealthController.GClass3008.GClass3019_0.MedEffect.MedKitStartDelay;
            ActiveHealthController.GClass3008.GClass3019_0.MedEffect.MedKitStartDelay = (float)CH_Plugin.HealDelay.Value;
            var newEffect = player.ActiveHealthController.DoMedEffect(__instance.MedsController_0.Item, EBodyPart.Common, 1f);
            if (newEffect == null)
            {
                __instance.State = Player.EOperationState.Finished;
                __instance.MedsController_0.FailedToApply = true;
                var callbackToRun = __instance.Callback_0;
                __instance.Callback_0 = null;
                callbackToRun(__instance.MedsController_0);
                ActiveHealthController.GClass3008.GClass3019_0.MedEffect.MedKitStartDelay = originalDelay;
                return false;
            }
            ;
            player.HealthController.EffectRemovedEvent += __instance.method_8;
            ActiveHealthController.GClass3008.GClass3019_0.MedEffect.MedKitStartDelay = originalDelay;

            if (CH_Plugin.ResetAnimation.Value && __instance.MedsController_0.Item is not MedicalItemClass)
            {
                Animation++;
                var variant = 0;
                if (__instance.MedsController_0.Item.TryGetItemComponent(out AnimationVariantsComponent animationVariantsComponent))
                {
                    variant = animationVariantsComponent.VariantsNumber;
                }

                var newAnim = (int)Mathf.Repeat((float)Animation, (float)variant);
#if DEBUG
                CH_Plugin.CH_Logger.LogWarning($"New anim: {newAnim}");
#endif

                if (__instance.MedsController_0.FirearmsAnimator != null)
                {
                    var mult = player.Skills.SurgerySpeed.Value / 100f;
                    var animator = __instance.MedsController_0.FirearmsAnimator;
                    
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
