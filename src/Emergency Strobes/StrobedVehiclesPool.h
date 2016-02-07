#pragma once

#include "StrobedVehicle.h"

using namespace Rage;
using namespace System::Collections::Generic;

namespace EmergencyStrobes 
{
	public ref class StrobedVehiclesPool
	{
	public:
		static void Initialize();
		static void Controller();
		static property GameFiber^ ControllerFiber { GameFiber^ get(); }
		static void Add(StrobedVehicle^ vehicle);
	    static void AddEmergencyVehicles();
		static bool ContainsVehicle(Vehicle^ vehicle);
		static bool ContainsStrobedVehicle(StrobedVehicle^ strobedVehicle);
	private:
		static GameFiber^ controllerFiber;
		static List<StrobedVehicle^>^ strobedVehicles;
		static List<Vehicle^>^ vehicles;
		static void OnFrameRender(System::Object^ sender, GraphicsEventArgs^ e);
	};
}
