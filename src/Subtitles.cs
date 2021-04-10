using ProtoBuf;
using System;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.GameContent;
using System.Linq;
using System.Collections.Generic;
using Vintagestory.Server;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

[assembly: ModInfo("Subtitles")]

namespace Subtitles {
  public class SubtitlesMod : ModSystem {
    private Harmony harmony;
    private GuiDialogSubtitles dialog;
    public override void StartClientSide(ICoreClientAPI capi) {
      base.StartClientSide(capi);
      capi.Logger.Debug("Subtitles - applying patches");
      harmony = new Harmony("goxmeor.Subtitles");
      harmony.PatchAll();
      capi.Logger.Debug("Subtitles - patches applied");

      Patch_ClientPlatformWindows_AudioCreate.capi = capi;
      Patch_ClientPlatformWindows_AudioCreate.subtitlesMod = this;

      // using IsPlayerReady just like HudClock does
      capi.Event.IsPlayerReady += (ref EnumHandling handling) => {
        capi.Logger.Debug("Subtitles - creating and opening dialog");
        dialog = new GuiDialogSubtitles(capi, this);
        dialog.TryOpen();
        return true;
      };

    }
    public void OnNewSound(string name, double yaw, double volume) {
      dialog.subtitleElement.OnNewSound(name, yaw, volume);
    }
  }

  [HarmonyPatch(typeof(ClientPlatformWindows))]
  [HarmonyPatch("AudioCreate")]
  public class Patch_ClientPlatformWindows_AudioCreate {
    public static readonly double AUDIBILITY_FACTOR = 1.5;
    public static ICoreClientAPI capi;
    public static SubtitlesMod subtitlesMod;
    public static void Prefix(
      SoundParams sound,
      AudioData data
    ) {
      if (sound.SoundType == EnumSoundType.Music) { return; }


      var locationPath = sound.Location.Path;
      var soundId = locationPath.StartsWith("sounds/") && locationPath.EndsWith(".ogg") ? locationPath.Substring(7, locationPath.Length - 7 - 4) : locationPath;

      // strip last character if it's a number (e.g. "block/dirt1", "block/dirt2", "block/dirt3", "block/dirt4")
      var lastChar = soundId.ToCharArray(soundId.Length - 1, 1)[0];
      if (lastChar >= '0' && lastChar <= '9') {
        soundId = soundId.Substring(0, soundId.Length - 1);
      }

      var name = Lang.GetIfExists("subtitles:" + soundId);
      if (name == "") { return; }
      if (name == null) {
        name = soundId;
        capi.Logger.Debug($"Subtitles: missing lang entry --> \"subtitles:{soundId}\": \"TODO\",");
      }

      var player = capi.World.Player;
      if (player == null) { return; }
      var playerPos = player.Entity.Pos.AsBlockPos;

      if (sound.Position == null || (sound.Position.X == 0 && sound.Position.Y == 0 && sound.Position.Z == 0)) {
        subtitlesMod.OnNewSound(name, Double.NaN, sound.Volume);
        return;
      }

      var dx = sound.Position.X - playerPos.X;
      var dy = sound.Position.Y - playerPos.Y;
      var dz = sound.Position.Z - playerPos.Z;

      // if the source of the sound is very close, do not treat it as directional
      if (Math.Abs(dx) < 2 && Math.Abs(dy) < 2 && Math.Abs(dz) < 2) {
        subtitlesMod.OnNewSound(name, Double.NaN, sound.Volume);
        return;
      }

      var yaw = Math.Atan2(dz, dx);
      var dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);

      // if it's out of "range", don't display it
      if (dist <= sound.Range * AUDIBILITY_FACTOR) {
        
      }

      subtitlesMod.OnNewSound(name, yaw, sound.Volume);
    }
  }
}
