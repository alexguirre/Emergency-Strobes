#pragma once

using namespace Rage;

namespace EmergencyStrobes
{
	public ref class StrobedVehicle
	{
	public:
		StrobedVehicle(Rage::Vehicle^ vehicle);
		property Rage::Vehicle^ Vehicle
		{ 
			Rage::Vehicle^ get();
			void set(Rage::Vehicle^ value);
		}
		void MainController();
		void HeadlightsController();
		void IndicatorsController();
		void ResetLights();
		bool IsHeadlightsControllerFiberAborted;
		bool IsIndicatorsControllerFiberAborted;
	private:
		GameFiber^ headlightsControllerFiber;
		GameFiber^ indicatorsControllerFiber;
		Rage::Vehicle^ vehicle;
		bool active;
		bool isRightHeadlightBroken;
		bool isLeftHeadlightBroken;
		bool isRightHeadlightDeformated;
		bool isLeftHeadlightDeformated;
		bool finishedHeadlightsFlash;
	};
}