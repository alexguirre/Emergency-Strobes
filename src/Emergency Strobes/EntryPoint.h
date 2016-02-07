#pragma once

using namespace System;
using namespace Rage;
using namespace Rage::Native;

namespace EmergencyStrobes
{

	public ref class EntryPoint
	{
	public:
		static void Main();
		static void HeadlightsController();
		static void IndicatorLightsController();
		static void ResetPlayerVehicleLights();
		static Vehicle^ PlayerVehicle;
	private:
		static bool manualActivate = false;
		static bool autoActivate = false;
		static bool isRightHeadlightBroken = false;
		static bool isLeftHeadlightBroken = false;
		static bool isRightHeadlightDeformated = false;
		static bool isLeftHeadlightDeformated = false;
		static bool finishedHeadlightsFlash = true;
		static bool disableAutoActivateManually = false;
		static bool canPlayerVehBeUsed = false;
	};
}
