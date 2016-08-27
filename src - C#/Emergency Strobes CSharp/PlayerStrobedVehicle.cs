namespace Emergency_Strobes
{
    // System
    using System;
    using System.Drawing;
    using System.Reflection;

    // RPH
    using Rage;
    using Rage.Native;
    using Graphics = Rage.Graphics;

    internal class PlayerStrobedVehicle
    {
        public const float HeadlightsDeformationThreshold = 0.0022225f;

        public readonly Vehicle Vehicle;
        public Pattern Pattern;
        public int PatternIndex;

        private int stagesCount;

        private Pattern.Stage currentStage;
        private int currentStageIndex;
        private uint currentStageStartTime;

        private bool active;
        private bool manuallyActive;
        private bool manualDisable;

        private bool isPlayerInVehicle;

        private bool shouldLeftHeadlightBeBroken, shouldRightHeadlightBeBroken;
        private Vector3 leftHeadlightOffset, rightHeadlightOffset;

        public PlayerStrobedVehicle(Vehicle veh, int patternIndex)
        {
            Vehicle = veh;
            PatternIndex = patternIndex;
            Pattern = Settings.Patterns[PatternIndex];
            stagesCount = Pattern.Stages.Length;
            if (Settings.ShowUI)
            {
                InitUI();
                Game.RawFrameRender += OnRawFrameRender;
            }

            Vector3 headlightLeftPos = veh.HasBone("headlight_l") ? veh.GetBonePosition("headlight_l") : Vector3.Zero;
            Vector3 headlightRightPos = veh.HasBone("headlight_r") ? veh.GetBonePosition("headlight_r") : Vector3.Zero;

            // if a headlight is broken it returns position Vector3 around Vector3.Zero, if so get an approximate offset
            if (headlightLeftPos.DistanceTo(Vector3.Zero) < 1.25f)
            {
                Vector3 leftPosOffset = veh.GetPositionOffset(veh.LeftPosition);
                Vector3 frontPosOffset = veh.GetPositionOffset(veh.FrontPosition);

                leftHeadlightOffset = new Vector3(leftPosOffset.X, frontPosOffset.Y, frontPosOffset.Z);
            }
            else
            {
                leftHeadlightOffset = veh.GetPositionOffset(headlightLeftPos);
            }

            if (headlightRightPos.DistanceTo(Vector3.Zero) < 1.25f)
            {
                Vector3 rightPosOffset = veh.GetPositionOffset(veh.RightPosition);
                Vector3 frontPosOffset = veh.GetPositionOffset(veh.FrontPosition);

                rightHeadlightOffset = new Vector3(rightPosOffset.X, frontPosOffset.Y, frontPosOffset.Z);
            }
            else
            {
                rightHeadlightOffset = veh.GetPositionOffset(headlightRightPos);
            }
        }

        public void ChangePattern(int newPatternIndex)
        {
            PatternIndex = newPatternIndex;
            Pattern = Settings.Patterns[PatternIndex];
            Game.DisplaySubtitle($"~b~[Emergency Strobes]~s~ Switching to pattern ~y~{Pattern.Name}~s~");
            stagesCount = Pattern.Stages.Length;
            ChangeStage(0);
            if ((active || manuallyActive) && !manualDisable)
                UpdateVehicleToCurrentStage();
            if (Settings.ShowUI)
                RecalculateUICurrentPatternText();
            if (Settings.PlaySwitchSounds)
                Settings.SwitchSound.Play();
            ShowUI(12.5);
        }

        public void Update()
        {
            isPlayerInVehicle = Game.LocalPlayer.Character.IsInVehicle(Vehicle, false);

            bool prevActive = active;
            active = Vehicle.IsSirenOn;

            bool prevManuallyActive = manuallyActive;
            if (isPlayerInVehicle && !active && Control.Toggle.IsJustPressed())
                manuallyActive = !manuallyActive;


            if (active != prevActive || manuallyActive != prevManuallyActive)
            {
                if (!active)
                    manualDisable = false;

                if (active || manuallyActive)
                {
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 2);
                    shouldLeftHeadlightBeBroken = Vehicle.GetDeformationAt(leftHeadlightOffset).LengthSquared() > HeadlightsDeformationThreshold;
                    shouldRightHeadlightBeBroken = Vehicle.GetDeformationAt(rightHeadlightOffset).LengthSquared() > HeadlightsDeformationThreshold;
                    UpdateVehicleToCurrentStage();
                    ShowUI(12.5);
                }
                else
                {
                    if (!shouldLeftHeadlightBeBroken)
                        Vehicle.SetLeftHeadlightBroken(false);
                    if (!shouldRightHeadlightBeBroken)
                        Vehicle.SetRightHeadlightBroken(false);
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 0);
                }
            }

            bool prevManualDisable = manualDisable;
            if (isPlayerInVehicle && active && Control.Toggle.IsJustPressed())
                manualDisable = !manualDisable;

            if (manualDisable != prevManualDisable)
            {
                if (manualDisable)
                {
                    if (!shouldLeftHeadlightBeBroken)
                        Vehicle.SetLeftHeadlightBroken(false);
                    if (!shouldRightHeadlightBeBroken)
                        Vehicle.SetRightHeadlightBroken(false);
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 0);
                }
                else
                {
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 2);
                    shouldLeftHeadlightBeBroken = Vehicle.GetDeformationAt(leftHeadlightOffset).LengthSquared() > HeadlightsDeformationThreshold;
                    shouldRightHeadlightBeBroken = Vehicle.GetDeformationAt(rightHeadlightOffset).LengthSquared() > HeadlightsDeformationThreshold;
                    UpdateVehicleToCurrentStage();
                }
            }

            if ((active || manuallyActive) && !manualDisable)
            {
                if (NeedsToChangeStage())
                {
                    shouldLeftHeadlightBeBroken = Vehicle.GetDeformationAt(leftHeadlightOffset).LengthSquared() > HeadlightsDeformationThreshold;
                    shouldRightHeadlightBeBroken = Vehicle.GetDeformationAt(rightHeadlightOffset).LengthSquared() > HeadlightsDeformationThreshold;

                    int newStageIndex = currentStageIndex + 1;
                    if (newStageIndex >= stagesCount)
                        newStageIndex = 0;

                    ChangeStage(newStageIndex);

                    UpdateVehicleToCurrentStage();
                }
                Vehicle.SetBrakeLights(false);
            }

            if (Control.SwitchPattern.IsJustPressed())
            {
                int newIndex = PatternIndex + 1;
                if (newIndex >= Settings.Patterns.Length)
                    newIndex = 0;

                ChangePattern(newIndex);
            }
        }

        public void ResetVehicleLights()
        {
            if (!shouldLeftHeadlightBeBroken)
                Vehicle.SetLeftHeadlightBroken(false);
            if (!shouldRightHeadlightBeBroken)
                Vehicle.SetRightHeadlightBroken(false);
            NativeFunction.Natives.SetVehicleLights(Vehicle, 0);
        }

        public void CleanUp()
        {
            if (Settings.ShowUI)
                Game.RawFrameRender -= OnRawFrameRender;
        }


        private void ChangeStage(int newIndex)
        {
            currentStageIndex = newIndex;
            currentStage = Pattern.Stages[currentStageIndex];
            currentStageStartTime = EntryPoint.GameTime;
        }

        private void UpdateVehicleToCurrentStage()
        {
            switch (Pattern.Stages[currentStageIndex].Type)
            {
                case PatternStageType.None:
                    Vehicle.SetLeftHeadlightBroken(true);
                    Vehicle.SetRightHeadlightBroken(true);
                    break;
                case PatternStageType.BothHeadlights:
                    if (!shouldLeftHeadlightBeBroken)
                        Vehicle.SetLeftHeadlightBroken(false);
                    if (!shouldRightHeadlightBeBroken)
                        Vehicle.SetRightHeadlightBroken(false);
                    break;
                case PatternStageType.LeftHeadlight:
                    Vehicle.SetLeftHeadlightBroken(true);
                    if (!shouldRightHeadlightBeBroken)
                        Vehicle.SetRightHeadlightBroken(false);
                    break;
                case PatternStageType.RightHeadlight:
                    if (!shouldLeftHeadlightBeBroken)
                        Vehicle.SetLeftHeadlightBroken(false);
                    Vehicle.SetRightHeadlightBroken(true);
                    break;
            }
        }

        private bool NeedsToChangeStage()
        {
            return EntryPoint.GameTime - currentStageStartTime > currentStage.Milliseconds;
        }

        #region UI
        public void ShowUI(double seconds)
        {
            if (!Settings.ShowUI)
                return;
            uiRemainingSeconds = seconds;
            uiStartShowingTime = DateTime.UtcNow;
        }

        private DateTime uiStartShowingTime;
        private double uiRemainingSeconds = 0.0f;
        private RectangleF uiBackgroundRectangle = new RectangleF(1920 - 360, 0, 280, 100).ConvertToCurrentCoordSystem();
        private Color uiBackgroundRectangleColor = Color.FromArgb(200, Color.Black);
        private RectangleF uiLeftHeadlightRectangle = new RectangleF(1920 - 360 + 14, 32, 32, 32).ConvertToCurrentCoordSystem();
        private RectangleF uiRightHeadlightRectangle = new RectangleF(1920 - 360 + 280 - 32 - 14, 32, 32, 32).ConvertToCurrentCoordSystem();

        private Color onColor = Color.Orange;
        private Color offColor = Color.FromArgb(20, 20, 20);
        private Color textColor = Color.White;

        private PointF uiTitlePosition;
        private string uiTitleText = $"Emergency Strobes v{Assembly.GetExecutingAssembly().GetName().Version}";
        private string uiTitleFont = Settings.UIFontName;
        private float uiTitleFontSize = 14.0f;

        private PointF uiCurrentPatternPosition;
        private string uiCurrentPatternText;
        private string uiCurrentPatternFont = Settings.UIFontName;
        private float uiCurrentPatternFontSize = 16.175f;

        private PointF uiHelpTipPosition;
        private string uiHelpTipText = $"Press {Settings.SwitchPatternKey} to change the pattern";
        private string uiHelpTipFont = Settings.UIFontName;
        private float uiHelpTipFontSize = 12.0f;

        private void InitUI()
        {
            SizeF textSize = Graphics.MeasureText(uiTitleText, uiTitleFont, uiTitleFontSize);
            float x = uiBackgroundRectangle.X, y = uiBackgroundRectangle.Y;
            x = uiBackgroundRectangle.X + uiBackgroundRectangle.Width * 0.5f - textSize.Width * 0.5f;
            uiTitlePosition = new PointF(x, y);


            textSize = Graphics.MeasureText(uiHelpTipText, uiHelpTipFont, uiHelpTipFontSize);
            x = uiBackgroundRectangle.X + uiBackgroundRectangle.Width * 0.5f - textSize.Width * 0.5f;
            y = uiBackgroundRectangle.Bottom - textSize.Height * 1.725f;
            uiHelpTipPosition = new PointF(x, y);

            RecalculateUICurrentPatternText();
        }

        private void RecalculateUICurrentPatternText()
        {
            uiCurrentPatternText = Pattern.Name.ToUpper();

            SizeF textSize = Graphics.MeasureText(uiCurrentPatternText, uiCurrentPatternFont, uiCurrentPatternFontSize);
            float x = uiBackgroundRectangle.X, y = uiBackgroundRectangle.Y;
            x = uiBackgroundRectangle.X + uiBackgroundRectangle.Width * 0.5f - textSize.Width * 0.5f;
            y = uiBackgroundRectangle.Y + uiBackgroundRectangle.Height * 0.5f - textSize.Height * 0.8f;
            uiCurrentPatternPosition = new PointF(x, y);
        }

        private void OnRawFrameRender(object sender, GraphicsEventArgs e)
        {
            if (isPlayerInVehicle && (DateTime.UtcNow - uiStartShowingTime).TotalSeconds <= uiRemainingSeconds)
            {
                Graphics g = e.Graphics;
                g.DrawRectangle(uiBackgroundRectangle, uiBackgroundRectangleColor);

                if ((active || manuallyActive) && !manualDisable)
                {
                    switch (currentStage.Type)
                    {
                        case PatternStageType.None:
                            g.DrawRectangle(uiLeftHeadlightRectangle, offColor);
                            g.DrawRectangle(uiRightHeadlightRectangle, offColor);
                            break;
                        case PatternStageType.BothHeadlights:
                            g.DrawRectangle(uiLeftHeadlightRectangle, onColor);
                            g.DrawRectangle(uiRightHeadlightRectangle, onColor);
                            break;
                        case PatternStageType.LeftHeadlight:
                            g.DrawRectangle(uiLeftHeadlightRectangle, onColor);
                            g.DrawRectangle(uiRightHeadlightRectangle, offColor);
                            break;
                        case PatternStageType.RightHeadlight:
                            g.DrawRectangle(uiLeftHeadlightRectangle, offColor);
                            g.DrawRectangle(uiRightHeadlightRectangle, onColor);
                            break;
                    }
                }
                else
                {
                    g.DrawRectangle(uiLeftHeadlightRectangle, offColor);
                    g.DrawRectangle(uiRightHeadlightRectangle, offColor);
                }

                UIDrawRectangleBorders(g, uiLeftHeadlightRectangle, 2.5f, Color.Black);
                UIDrawRectangleBorders(g, uiRightHeadlightRectangle, 2.5f, Color.Black);
                UIDrawRectangleBorders(g, uiBackgroundRectangle, 2.5f, Color.Black);

                g.DrawText(uiTitleText, uiTitleFont, uiTitleFontSize, uiTitlePosition, textColor);
                g.DrawText(uiCurrentPatternText, uiCurrentPatternFont, uiCurrentPatternFontSize, uiCurrentPatternPosition, textColor);
                g.DrawText(uiHelpTipText, uiHelpTipFont, uiHelpTipFontSize, uiHelpTipPosition, textColor);
            }
        }

        private static void UIDrawRectangleBorders(Graphics g, RectangleF rectangle, float borderWidth, Color color)
        {
            g.DrawRectangle(new RectangleF(rectangle.X - borderWidth, rectangle.Y - borderWidth, borderWidth, rectangle.Height + borderWidth * 2f), color); // left
            g.DrawRectangle(new RectangleF(rectangle.X, rectangle.Y - borderWidth, rectangle.Width, borderWidth), color); // top
            g.DrawRectangle(new RectangleF(rectangle.Right, rectangle.Y - borderWidth, borderWidth, rectangle.Height + borderWidth * 2f), color); // right
            g.DrawRectangle(new RectangleF(rectangle.X, rectangle.Bottom, rectangle.Width, borderWidth), color); // bottom
        }
        #endregion // UI
    }
}
