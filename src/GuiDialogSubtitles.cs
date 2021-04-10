using Vintagestory.API.Client;

namespace Subtitles {
  class GuiDialogSubtitles : HudElement {
    public override string ToggleKeyCombinationCode => null;
    private SubtitlesMod subtitlesMod;
    public GuiElementSubtitleList subtitleElement;
    public GuiDialogSubtitles(ICoreClientAPI capi, SubtitlesMod subtitlesMod) : base(capi) {
      this.subtitlesMod = subtitlesMod;
      SetupDialog();
    }
    private void SetupDialog() {
      ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightBottom).WithFixedPadding(10);
      SingleComposer = capi.Gui.CreateCompo("subtitles", dialogBounds);

      ElementBounds subtitleListBounds = ElementBounds.FixedSize(250, 450);
      dialogBounds.WithChild(subtitleListBounds);
      subtitleElement = new GuiElementSubtitleList(SingleComposer.Api, subtitleListBounds);
      // var bar = new GuiElementDynamicText();
      SingleComposer.AddInteractiveElement(subtitleElement, "subtitleList");

      SingleComposer.Compose(false);
    }

    public override bool OnEscapePressed() {
      return base.OnEscapePressed();
    }
  }
}
