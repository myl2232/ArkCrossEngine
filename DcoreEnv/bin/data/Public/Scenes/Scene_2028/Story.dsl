﻿story(1)
{
	local
	{
		@rnd(0);
		@hilas(0);
	};
	onmessage("start")
	{
		wait(100);
		loop(7)
		{
			createnpc(1001+$$);
		};
		wait(1000);
		setblockedshader(0x0000ff90,0.5,0,0xff000090,0.5,0);
	};
	onmessage("allnpckilled")
	{
		inc(@hilas);
		wait(600);
		if(1==@hilas)
		{
			loop(8)
			{
				createnpc(1101+$$);
			};
			wait(1000);
			setblockedshader(0x0000ff90,0.5,0,0xff000090,0.5,0);
		};
		if(2==@hilas)
		{
			wait(600);
			publishlogicevent("ge_area_clear","game",0);
			wait(1500);
			showwall("AtoB",false);
			wait(100);
			restartareamonitor(2);
		};
	};
	onmessage("anyuserenterarea",2)
	{
		showwall("BDoor",true);
		startstory(2);
		terminate();	
	};
onmessage("missionfailed")
{
changescene(0);
terminate();
};
};
story(2)
{

	local
	{
	@hilas(0);
	};
	onmessage("start")
	{
		wait(100);
	loop(7)
	{
	createnpc(2001+$$);
	};
	wait(5000);
	setblockedshader(0x0000ff90,0.5,0,0xff000090,0.5,0);
	};
	onmessage("allnpckilled")
	{
	inc(@hilas);
		wait(600);
		if(1==@hilas)
		{
			loop(5)
			{
				createnpc(2101+$$);
			};
			wait(1000);
			setblockedshader(0x0000ff90,0.5,0,0xff000090,0.5,0);
		};
		if(2==@hilas)
		{
		wait(600);
		publishlogicevent("ge_area_clear","game",0);
		wait(1500);
		showwall("BtoC",false);
		wait(100);
		restartareamonitor(3);
		};
	};
	onmessage("anyuserenterarea",3)
	{
		showwall("CDoor",true);
		startstory(3);
		terminate();
	};
onmessage("missionfailed")
{
changescene(0);
terminate();
};
};
story(3)
{
local
{
@reconnectCount(0);
@hilas(0);
};
	onmessage("start")
	{	
		wait(100);
	loop(10)
	{
	createnpc(3001+$$);
	};
	wait(1000);
	setblockedshader(0x0000ff90,0.5,0,0xff000090,0.5,0);
	};
	onmessage("allnpckilled")
	{
	inc(@hilas);
	wait(600);
	if(1==@hilas)
	{
	loop(2)
	{
	createnpc(3101+$$);
	};
	wait(1000);
	setblockedshader(0x0000ff90,0.5,0,0xff000090,0.5,0);
	};
	if(2==@hilas)
	{
	lockframe(0.01);
wait(500);
lockframe(0.05);
wait(1800);
lockframe(0.08);
wait(300);
lockframe(0.2);
wait(500);
lockframe(1.0);
		wait(300);
		//camerayaw(0,100);
	//cameraheight(-1,100);
	//cameradistance(-1,100);
	wait(1000);
	publishlogicevent("ge_area_clear","game",1);
	wait(2000);
		loop(10)
		{
		//检测网络状态
		while(!islobbyconnected()&&@reconnectCount<10)
		{
		reconnectlobby();
		wait(3000);
		inc(@reconnectCount);
		loop(10)
		{
		if(islobbylogining())
		{
		wait(1000);
		};
		};
		if(islobbylogining())
		{
		disconnectlobby();
		};
		};
		if(islobbyconnected()&&!islobbylogining())
		{
		missioncompleted(0);
		wait(21000);
		disconnectlobby();
		}else{
		wait(10000);
		//terminate();
		};
		};
		changescene(0);
		terminate();
		};
	};
onmessage("missionfailed")
{
changescene(0);
terminate();
};
};
