#include "stdafx.h"

#include "StrobedVehicle.h"
#include "Common.h"
#include "Settings.h"
#include "EntryPoint.h"

using namespace EmergencyStrobes;
using namespace Rage;

StrobedVehicle::StrobedVehicle(Rage::Vehicle^ vehicle)
{
	this->vehicle = vehicle;

	IsHeadlightsControllerFiberAborted = false;
	IsIndicatorsControllerFiberAborted = false;

	if(Settings::AreAIStrobesHeadlightsEnabled)
		headlightsControllerFiber = GameFiber::StartNew(gcnew Threading::ThreadStart(this, &StrobedVehicle::HeadlightsController));

	if (Settings::AreAIStrobesIndicatorLightsEnabled)
		indicatorsControllerFiber = GameFiber::StartNew(gcnew Threading::ThreadStart(this, &StrobedVehicle::IndicatorsController));
}


Rage::Vehicle^ StrobedVehicle::Vehicle::get()
{
	return this->vehicle;
}

void StrobedVehicle::Vehicle::set(Rage::Vehicle^ vehicle) 
{
	this->vehicle = vehicle;
}


void StrobedVehicle::MainController()
{
	if (!active && EntityExtensions::Exists(vehicle) && vehicle->IsSirenOn && (vehicle->HasDriver || Settings::AIStrobesStayOnWithoutDriver))
	{
		active = true;
	}
	else if (active && EntityExtensions::Exists(vehicle) && (!vehicle->IsSirenOn || (!vehicle->HasDriver || !Settings::AIStrobesStayOnWithoutDriver)))
	{
		active = false;
	}


	if (!active && finishedHeadlightsFlash && EntityExtensions::Exists(vehicle))
	{
		isLeftHeadlightBroken = Common::IsVehicleLeftHeadlightBroken(vehicle);
		isRightHeadlightBroken = Common::IsVehicleRightHeadlightBroken(vehicle);
	}
}

void StrobedVehicle::HeadlightsController()
{
	unsigned int headlightsInt = 0;
	while (true)
	{
		GameFiber::Yield();

		if (EntityExtensions::Exists(vehicle) && vehicle != Game::LocalPlayer->Character->CurrentVehicle)
		{
			if (active)
			{
				finishedHeadlightsFlash = false;

				isLeftHeadlightDeformated = Common::GetVehicleDeformationAtFrontLeft(vehicle).Length() > 0.01225f;
				isRightHeadlightDeformated = Common::GetVehicleDeformationAtFrontRight(vehicle).Length() > 0.01225f;

				NativeFunction::CallByName<unsigned int>((String ^)"SET_VEHICLE_LIGHT_MULTIPLIER", gcnew array<NativeArgument^> { vehicle, Settings::HeadlightsBrightness });
				NativeFunction::CallByName<unsigned int>((String ^)"SET_VEHICLE_LIGHTS", gcnew array<NativeArgument^> { vehicle, 2 });
				if (headlightsInt % 2 == 0)
				{
					Common::SetVehicleLeftHeadlightBroken(vehicle, true);

					if (!isRightHeadlightBroken && !isRightHeadlightDeformated)
						Common::SetVehicleRightHeadlightBroken(vehicle, false);
				}
				else
				{
					if (!isLeftHeadlightBroken && !isLeftHeadlightDeformated)
						Common::SetVehicleLeftHeadlightBroken(vehicle, false);

					Common::SetVehicleRightHeadlightBroken(vehicle, true);
				}
				headlightsInt++;
				vehicle->IsEngineOn = true;
				GameFiber::Sleep(Settings::HeadlightsInterval);
			}
			else if (EntityExtensions::Exists(vehicle) && headlightsInt != 0)
			{
				headlightsInt = 0;

				if (!isLeftHeadlightBroken && !isLeftHeadlightDeformated)
					Common::SetVehicleLeftHeadlightBroken(vehicle, false);
				if (!isRightHeadlightBroken && !isRightHeadlightDeformated)
					Common::SetVehicleRightHeadlightBroken(vehicle, false);

				NativeFunction::CallByName<unsigned int>((String ^)"SET_VEHICLE_LIGHTS", gcnew array<NativeArgument^> { vehicle, 0 });
				NativeFunction::CallByName<unsigned int>((String ^)"SET_VEHICLE_LIGHT_MULTIPLIER", gcnew array<NativeArgument^> { vehicle, 1.0f });

				finishedHeadlightsFlash = true;
			}
		}
		else
		{
			break;
		}
	}

	if (headlightsControllerFiber != nullptr)
	{
		IsHeadlightsControllerFiberAborted = true;
		headlightsControllerFiber->Abort();
	}
}

void StrobedVehicle::IndicatorsController()
{
	unsigned int indicatorlightsInt = 0;
	while (true)
	{
		GameFiber::Yield();

		if (EntityExtensions::Exists(vehicle) && vehicle != EntryPoint::PlayerVehicle)
		{
			if (active)
			{
				if (indicatorlightsInt % 2 == 0)
				{
					vehicle->IndicatorLightsStatus = VehicleIndicatorLightsStatus::LeftOnly;
				}
				else
				{
					vehicle->IndicatorLightsStatus = VehicleIndicatorLightsStatus::RightOnly;
				}
				indicatorlightsInt++;
				GameFiber::Sleep(Settings::IndicatorLightsInterval);
			}
			else if (EntityExtensions::Exists(vehicle) && indicatorlightsInt != 0)
			{
				indicatorlightsInt = 0;
				vehicle->IndicatorLightsStatus = VehicleIndicatorLightsStatus::Off;
			}
		}
		else
		{
			break;
		}
	}

	if (indicatorsControllerFiber != nullptr)
	{
		IsIndicatorsControllerFiberAborted = true;
		indicatorsControllerFiber->Abort();
	}
}

void StrobedVehicle::ResetLights()
{
	if (EntityExtensions::Exists(vehicle))
	{
		vehicle->IndicatorLightsStatus = VehicleIndicatorLightsStatus::Off;

		if (!isLeftHeadlightBroken && !isRightHeadlightDeformated)
			Common::SetVehicleLeftHeadlightBroken(vehicle, false);

		if (!isRightHeadlightBroken && !isRightHeadlightDeformated)
			Common::SetVehicleRightHeadlightBroken(vehicle, false);

		NativeFunction::CallByName<unsigned int>((String ^)"SET_VEHICLE_LIGHTS", gcnew array<NativeArgument^> { vehicle, 0 });
		NativeFunction::CallByName<unsigned int>((String ^)"SET_VEHICLE_LIGHT_MULTIPLIER", gcnew array<NativeArgument^> { vehicle, 1.0f });
	}
}