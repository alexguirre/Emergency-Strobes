#pragma once

using namespace System;
using namespace Rage;
using namespace Rage::Native;

namespace EmergencyStrobes
{

	public ref class Common
	{
	public:
		static void SetVehicleLeftHeadlightBroken(Vehicle^ veh, bool broken);
		static void SetVehicleRightHeadlightBroken(Vehicle^ veh, bool broken);
		static bool IsVehicleLeftHeadlightBroken(Vehicle^ veh);
		static bool IsVehicleRightHeadlightBroken(Vehicle^ veh);
		static Vector3 GetVehicleDeformationAtFrontLeft(Vehicle^ veh);
		static Vector3 GetVehicleDeformationAtFrontRight(Vehicle^ veh);
		static String^ GetCurrentVersion();
	};
}
