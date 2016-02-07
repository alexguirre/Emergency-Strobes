#include "stdafx.h"

#include "StrobedVehiclesPool.h"
#include "StrobedVehicle.h"
#include "Common.h"
#include "Settings.h"
#include "EntryPoint.h"

using namespace EmergencyStrobes;
using namespace Rage;
using namespace Rage::Native;


void StrobedVehiclesPool::Initialize()
{
	strobedVehicles = gcnew List<StrobedVehicle^>();
	vehicles = gcnew List<Vehicle^>();
	controllerFiber = GameFiber::StartNew(gcnew Threading::ThreadStart(&Controller), "StrobedVehiclesPool::Controller() Fiber");
	Game::FrameRender += gcnew System::EventHandler<Rage::GraphicsEventArgs ^>(&EmergencyStrobes::StrobedVehiclesPool::OnFrameRender);
}

GameFiber^ StrobedVehiclesPool::ControllerFiber::get() 
{
	return controllerFiber;
}

void StrobedVehiclesPool::Controller()
{
	while (true)
	{
		GameFiber::Sleep(500);

		AddEmergencyVehicles();
	}
}

void StrobedVehiclesPool::Add(StrobedVehicle^ strobedVeh)
{
	if (strobedVeh == nullptr || !EntityExtensions::Exists(strobedVeh->Vehicle))
	{
		Game::LogTrivial("StrobedVehiclesPool - Vehicle is invalid, aborting addition");
		return;
	}
	strobedVehicles->Add(strobedVeh);
	vehicles->Add(strobedVeh->Vehicle);
}

void StrobedVehiclesPool::AddEmergencyVehicles() 
{
	array<Entity^>^ vehiclesEntities = World::GetEntities(GetEntitiesFlags::ConsiderAllVehicles | GetEntitiesFlags::ExcludePlayerVehicleIfDriver);
	for (int i = 0; i < vehiclesEntities->Length; i++)
	{
		if (EntityExtensions::Exists(vehiclesEntities[i]) && vehiclesEntities[i]->Model.IsVehicle)
		{
			Vehicle^ veh = (Vehicle^)vehiclesEntities[i];
			if (veh->HasSiren && !ContainsVehicle(veh) && !Settings::ExcludedModels->Contains(veh->Model) && veh != EntryPoint::PlayerVehicle)
			{
				strobedVehicles->Add(gcnew StrobedVehicle(veh));
				vehicles->Add(veh);
				Game::LogTrivial("StrobedVehiclesPool - Vehicle added");
			}
		}
	}
}

bool StrobedVehiclesPool::ContainsVehicle(Vehicle^ vehicle)
{
	return vehicles->Contains(vehicle);
}

bool StrobedVehiclesPool::ContainsStrobedVehicle(StrobedVehicle^ strobedVehicle)
{
	return strobedVehicles->Contains(strobedVehicle);
}



void StrobedVehiclesPool::OnFrameRender(System::Object^ sender, GraphicsEventArgs^ e)
{
	for (int i = 0; i < strobedVehicles->Count; i++)
	{
		strobedVehicles[i]->MainController();

		//if (EntityExtensions::Exists(strobedVehicles[i]->Vehicle))
		//{
		//	Vector3 pos = strobedVehicles[i]->Vehicle->Position + Vector3::WorldUp;
		//	array<NativeArgument^>^ arguments = gcnew array<NativeArgument^>{ 20, pos.X, pos.Y, pos.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 0.5f, 255, 0, 255, 255, true, true, 2, false, 0, 0, false };
		//	NativeFunction::CallByName<unsigned int>((String^)"DRAW_MARKER", arguments);
		//}

		if (strobedVehicles[i] == nullptr || strobedVehicles[i]->IsHeadlightsControllerFiberAborted || strobedVehicles[i]->IsIndicatorsControllerFiberAborted || !EntityExtensions::Exists(strobedVehicles[i]->Vehicle) || strobedVehicles[i]->Vehicle == EntryPoint::PlayerVehicle)
		{
			Game::LogTrivial("StrobedVehiclesPool - Vehicle removed");
			if (EntityExtensions::Exists(strobedVehicles[i]->Vehicle))
			{
				strobedVehicles[i]->ResetLights();
				vehicles->Remove(strobedVehicles[i]->Vehicle);
			}
			strobedVehicles->RemoveAt(i);
		}
	}
}
