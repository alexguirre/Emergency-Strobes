namespace Emergency_Strobes
{
    // System
    using System;
    using System.Windows.Forms;
    using System.Drawing;
    using System.Reflection;

    // RPH
    using Rage;
    using Rage.Native;
    using Graphics = Rage.Graphics;

    internal class PlayerStrobedVehicle
    {
        public readonly Vehicle Vehicle;
        public Pattern Pattern;
        public int PatternIndex;

        private int stagesCount;

        private Pattern.Stage currentStage;
        private int currentStageIndex;
        private int currentStageRemainingTicks;

        private bool active;
        private bool manuallyActive;
        private bool manualDisable;

        private bool isPlayerInVehicle;

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
        }

        public void ChangePattern(int newPatternIndex)
        {
            PatternIndex = newPatternIndex;
            Pattern = Settings.Patterns[PatternIndex];
            Game.DisplaySubtitle($"~b~[Emergency Strobes]~s~ Switching to pattern ~y~{Pattern.Name}~s~");
            stagesCount = Pattern.Stages.Length;
            ChangeStage(0);
            if (active || manuallyActive)
                UpdateVehicleToCurrentStage();
            if (Settings.ShowUI)
                RecalculateUICurrentPatternText();
            ShowUI(12.5);
        }

        public void Update()
        {
            isPlayerInVehicle = Game.LocalPlayer.Character.IsInVehicle(Vehicle, false);

            bool prevActive = active;
            active = Vehicle.IsSirenOn;

            bool prevManuallyActive = manuallyActive;
            if (isPlayerInVehicle && !active && Game.IsKeyDown(Settings.ToggleKey))
                manuallyActive = !manuallyActive;


            if (active != prevActive || manuallyActive != prevManuallyActive)
            {
                if (!active)
                    manualDisable = false;

                if (active || manuallyActive)
                {
                    //NativeFunction.Natives.SetVehicleLightMultiplier(Vehicle, Settings.Brightness);
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 2);
                    UpdateVehicleToCurrentStage();
                    ShowUI(12.5);
                }
                else
                {
                    Vehicle.SetLeftHeadlightBroken(false);
                    Vehicle.SetRightHeadlightBroken(false);
                    //NativeFunction.Natives.SetVehicleLightMultiplier(Vehicle, 1.0f);
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 0);
                }
            }

            bool prevManualDisable = manualDisable;
            if (isPlayerInVehicle && active && Game.IsKeyDown(Settings.ToggleKey))
                manualDisable = !manualDisable;

            if (manualDisable != prevManualDisable)
            {
                if (manualDisable)
                {
                    Vehicle.SetLeftHeadlightBroken(false);
                    Vehicle.SetRightHeadlightBroken(false);
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 0);
                }
                else
                {
                    NativeFunction.Natives.SetVehicleLights(Vehicle, 2);
                    UpdateVehicleToCurrentStage();
                }
            }

            if ((active || manuallyActive) && !manualDisable)
            {
                if (NeedsToChangeStage())
                {
                    int newStageIndex = currentStageIndex + 1;
                    if (newStageIndex >= stagesCount)
                        newStageIndex = 0;

                    ChangeStage(newStageIndex);

                    UpdateVehicleToCurrentStage();
                }

                currentStageRemainingTicks--;
            }

            if (Game.IsKeyDown(Settings.SwitchPatternKey))
            {
                int newIndex = PatternIndex + 1;
                if (newIndex >= Settings.Patterns.Length)
                    newIndex = 0;

                ChangePattern(newIndex);
            }
        }

        public void ResetVehicleLights()
        {
            Vehicle.SetLeftHeadlightBroken(false);
            Vehicle.SetRightHeadlightBroken(false);
            //NativeFunction.Natives.SetVehicleLightMultiplier(Vehicle, 1.0f);
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
            currentStageRemainingTicks = currentStage.Ticks;
        }

        private void UpdateVehicleToCurrentStage()
        {
            switch (Pattern.Stages[currentStageIndex].Type)
            {
                case PatternStageType.None:
                    Vehicle.SetLeftHeadlightBroken(true);
                    Vehicle.SetRightHeadlightBroken(true);
                    break;
                case PatternStageType.Both:
                    Vehicle.SetLeftHeadlightBroken(false);
                    Vehicle.SetRightHeadlightBroken(false);
                    break;
                case PatternStageType.LeftOnly:
                    Vehicle.SetLeftHeadlightBroken(true);
                    Vehicle.SetRightHeadlightBroken(false);
                    break;
                case PatternStageType.RightOnly:
                    Vehicle.SetLeftHeadlightBroken(false);
                    Vehicle.SetRightHeadlightBroken(true);
                    break;
            }
        }

        private bool NeedsToChangeStage()
        {
            return currentStageRemainingTicks <= 0;
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
        private RectangleF uiLeftHeadlightRectangle = new RectangleF(1920 - 360 + 32, 32, 32, 32).ConvertToCurrentCoordSystem();
        private RectangleF uiRightHeadlightRectangle = new RectangleF(1920 - 360 + 280 - 64, 32, 32, 32).ConvertToCurrentCoordSystem();

        private Color onColor = Color.White;
        private Color offColor = Color.FromArgb(20, 20, 20);

        private PointF uiTitlePosition;
        private string uiTitleText = $"Emergency Strobes v{Assembly.GetExecutingAssembly().GetName().Version}";
        private string uiTitleFont = Settings.UIFontName;
        private float uiTitleFontSize = 14.0f;

        private PointF uiCurrentPatternPosition;
        private string uiCurrentPatternText;
        private string uiCurrentPatternFont = Settings.UIFontName;
        private float uiCurrentPatternFontSize = 17.0f;

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
                        case PatternStageType.Both:
                            g.DrawRectangle(uiLeftHeadlightRectangle, onColor);
                            g.DrawRectangle(uiRightHeadlightRectangle, onColor);
                            break;
                        case PatternStageType.LeftOnly:
                            g.DrawRectangle(uiLeftHeadlightRectangle, onColor);
                            g.DrawRectangle(uiRightHeadlightRectangle, offColor);
                            break;
                        case PatternStageType.RightOnly:
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

                g.DrawText(uiTitleText, uiTitleFont, uiTitleFontSize, uiTitlePosition, onColor);
                g.DrawText(uiCurrentPatternText, uiCurrentPatternFont, uiCurrentPatternFontSize, uiCurrentPatternPosition, onColor);
                g.DrawText(uiHelpTipText, uiHelpTipFont, uiHelpTipFontSize, uiHelpTipPosition, onColor);
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
