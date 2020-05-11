using System;
using System.Linq;
using System.Xml;

using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Craft.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

using ReflectionExtension;

namespace CraftingPasteModule {
    public class CraftingPasteSubModule: MBSubModuleBase {
        private CraftingState craftingState = null;

        protected override void OnSubModuleLoad() {
            ScreenManager.OnPushScreen += OnPushOrPopScreen;
            ScreenManager.OnPopScreen += OnPushOrPopScreen;
        }

        protected override void OnSubModuleUnloaded() {
            ScreenManager.OnPushScreen -= OnPushOrPopScreen;
            ScreenManager.OnPopScreen -= OnPushOrPopScreen;
        }

        protected override void OnApplicationTick(float dt) {
            if (craftingState != null) {
                if (Input.DebugInput.IsHotKeyPressed("Copy")) {
                    Input.SetClipboardText(craftingState.CraftingLogic.GetXmlCodeForCurrentItem(craftingState.CraftingLogic.GetCurrentCraftedItemObject()));
                    InformationManager.DisplayMessage(new InformationMessage("design copied to clipboard"));
                }
                if (Input.DebugInput.IsHotKeyPressed("Paste")) {
                    // alas, this doesn't help:
                    //      craftingState.CraftingLogic.SwitchToPiece(WeaponDesignElement.CreateUsablePiece(craftingState.CraftingLogic.CurrentCraftingTemplate.Pieces.First(p => p.StringId == "xxx"), scalePercentage));
                    //      craftingScreen.OnCraftingLogicRefreshed();
                    // gotta hack it, see WeaponDesignVM.ExecuteRandomize() for the reference
                    var craftingVM = ScreenManager.TopScreen.GetFieldValue<CraftingVM>("_dataSource"); // TopScreen us SandBox.GauntletUI.CraftingGauntletScreen in crafting state
                    if (craftingVM.IsInCraftingMode) {
                        var weaponDesignVM = craftingVM.WeaponDesign;
                        var xmlDocument = new XmlDocument();
                        try {
                            xmlDocument.LoadXml(Input.GetClipboardText().ToLowerInvariant()); // I know that XML is case-sensitive, but that's what TaleWorlds do in character appearance editor
                            foreach (XmlNode pieceNode in xmlDocument.GetElementsByTagName("piece")) {
                                var pieceType = (CraftingPiece.PieceTypes)Enum.Parse(typeof(CraftingPiece.PieceTypes), pieceNode.Attributes["type"].Value, ignoreCase: true);
                                var pieceId = pieceNode.Attributes["id"].Value;
                                var pieceCraftPartVM = GetCraftPartVM(weaponDesignVM, pieceType, pieceId);
                                pieceCraftPartVM.CraftingPiece.ScalePercentage = int.Parse(pieceNode.Attributes["scale_factor"]?.Value ?? "100"); // apparently the 2nd argument to WeaponDesignVM.OnSetItemPart() has no effect beyond the UI
                                weaponDesignVM.InvokeMethod("OnSetItemPart", pieceCraftPartVM, 0, false);
                            }
                            weaponDesignVM.SetFieldValue("_updatePiece", false);
                            weaponDesignVM.RefreshItem();
                            weaponDesignVM.InvokeMethod("AddHistoryKey");
                            weaponDesignVM.SetFieldValue("_updatePiece", true);
                            InformationManager.DisplayMessage(new InformationMessage("design pasted from clipboard"));
                        } catch { }
                    }
                }
#if DEBUG
                // reference: Crafting.GetXmlCodeForCurrentItem()
                // opted for more complete representation of internals, as such weaponDesignElement.IsValid is ignored and
                // weaponDesignElement.CraftingPiece.StringId may be null - this will crash in such a weird way (in actual C code of Imgui
                // I reckon) that dnSpy is not even able to catch and you can't check backtrace etc
                Imgui.BeginMainThreadScope();
                Imgui.Begin("CraftingLogic.SelectedPieces");
                Imgui.Text($"value={craftingState.CraftingLogic.GetCurrentCraftedItemObject().Value}");
                Imgui.Columns(3);
                foreach (var weaponDesignElement in craftingState.CraftingLogic.SelectedPieces) {
                    Imgui.Text(weaponDesignElement.CraftingPiece.PieceType.ToString()); Imgui.NextColumn();
                    Imgui.Text(weaponDesignElement.CraftingPiece.StringId ?? "null"); Imgui.NextColumn();
                    Imgui.Text($"{weaponDesignElement.ScalePercentage}%%"); Imgui.NextColumn();
                }
                Imgui.End();
                Imgui.EndMainThreadScope();
#endif
            }
        }

        private void OnPushOrPopScreen(ScreenBase _) {
            craftingState = Game.Current?.GameStateManager.ActiveState as CraftingState;
        }

        // weaponDesignVM._pieceLists[pieceType] would be cleaner but it's private
        private CraftPartVM GetCraftPartVM(WeaponDesignVM weaponDesignVM, CraftingPiece.PieceTypes pieceType, string pieceId) {
            MBBindingList<CraftPartVM> partList = null;
            switch (pieceType) {
                case CraftingPiece.PieceTypes.Blade:
                    partList = weaponDesignVM.BladePartList;
                    break;
                case CraftingPiece.PieceTypes.Guard:
                    partList = weaponDesignVM.GuardPartList;
                    break;
                case CraftingPiece.PieceTypes.Handle:
                    partList = weaponDesignVM.HandlePartList;
                    break;
                case CraftingPiece.PieceTypes.Pommel:
                    partList = weaponDesignVM.PommelPartList;
                    break;
                case CraftingPiece.PieceTypes.Invalid:
                case CraftingPiece.PieceTypes.NumberOfPieceTypes:
                    break;
            }
            return partList.First(p => p.CraftingPiece.CraftingPiece.StringId == pieceId);
        }
    }
}
