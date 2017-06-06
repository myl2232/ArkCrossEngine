﻿story(1)
{ 
	local
	{
		@Count(0);
	};
	onmessage("start")
	{
	  wait(1900);
	  publishlogicevent("ge_set_story_state","game",0);
	  showwall("AtoB",false);
	  wait(100);
	  inc(@Count);
	  wait(100);
	  publishgfxevent("ge_pve_fightinfo","ui",1,1,1,5);
	  wait(10);
	  loop(4)
	  {
	  	createnpc(1001+$$);
	  	wait(300);
	  	npcpatrol(1001+$$,vector2list("19.70694 20.56544"),isnotloop);
	  };
	  wait(2000);
	  loop(4)
	  {
	  	createnpc(1005+$$);
	  	wait(300);
	  	npcpatrol(1005+$$,vector2list("19.70694 20.56544"),isnotloop);
	  };
	  wait(10);
	  wait(1000);
	  setblockedshader(0x0000ff90,0.5,0,0xff000090,0.5,0);
	};
	onmessage("allnpckilled")
	{
		wait(10);
		inc(@Count);
	  wait(2000);
	  publishgfxevent("ge_pve_fightinfo","ui",1,@Count,@Count,5);
	  wait(10);
	  if(2==@Count)
	  {
		  loop(3)
		  {
		  	createnpc(2001+$$);
		  	wait(100);
		  	npcpatrol(2001+$$,vector2list("19.70694 20.56544"),isnotloop);
		  };
		  wait(800);
		  loop(2)
		  {
		  	createnpc(2004+$$);
		  	wait(100);
		  	npcpatrol(2004+$$,vector2list("19.70694 20.56544"),isnotloop);
		  };
		  wait(1500);
		  loop(4)
		  {
		  	createnpc(2006+$$);
		  	wait(100);
		  	npcpatrol(2006+$$,vector2list("19.70694 20.56544"),isnotloop);
		  };
		};
		if(3==@Count)
	  {
		  loop(3)
		  {
		  	createnpc(3001+$$);
		  	wait(100);
		  	npcpatrol(3001+$$,vector2list("19.70694 20.56544"),isnotloop);
		  };
		  wait(300);
		  loop(2)
		  {
		  	createnpc(3004+$$);
		  	wait(100);
		  	npcpatrol(3004+$$,vector2list("19.70694 20.56544"),isnotloop);
		  };
		  wait(600);
		  createnpc(3006);
		  npcpatrol(3006,vector2list("19.70694 20.56544"),isnotloop);
		  loop(3)
		  {
		  	createnpc(3007+$$);
		  	wait(100);
		  	npcpatrol(3007+$$,vector2list("19.70694 20.56544"),isnotloop);
		  };
		  createnpc(3010);
		  npcpatrol(3010,vector2list("19.70694 20.56544"),isnotloop);
		};
		if(4==@Count)
	  {
		  loop(3)
		  {
		  	createnpc(4001+$$);
		  	wait(100);
		  	npcpatrol(4001+$$,vector2list("19.70694 20.56544"),isnotloop);
		  };
		  wait(1300);
		  loop(5)
		  {
		  	createnpc(4004+$$);
		  	wait(100);
		  	npcpatrol(4004+$$,vector2list("19.70694 20.56544"),isnotloop);
		  };
		  wait(2000);
		  loop(2)
		  {
		  	createnpc(4009+$$);
		  	wait(100);
		  	npcpatrol(4009+$$,vector2list("19.70694 20.56544"),isnotloop);
		  };
		  wait(10);
		  loop(2)
		  {
		  	createnpc(4011+$$);
		  	wait(100);
		  	npcpatrol(4011+$$,vector2list("19.70694 20.56544"),isnotloop);
		  };
		};
		if(5==@Count)
	  {
	  	wait(100);
	  	showwall("Def02",false);
	  	wait(10);
	  	showwall("Def03",false);
	  	wait(10);
	  	showwall("Def04",false);
	  	wait(10);
	  	showwall("Def05",false);
	  	wait(10);
	  	showwall("Def06",false);
	  	wait(10);
	  	showwall("Def07",false);
	  	wait(10);
	  	showwall("Def01",false);
			//showdlg(103401);
			wait(300);
			restartareamonitor(2);
		};
		wait(1000);
		setblockedshader(0x0000ff90,0.5,0,0xff000090,0.5,0);
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
    @reconnectCount(0);
  };
	onmessage("start")
	{	
		wait(100);
	  loop(11)
	  {
  	  createnpc(5001+$$);
  	};
  	wait(1000);
	  setblockedshader(0x0000ff90,0.5,0,0xff000090,0.5,0);
	};
	onmessage("allnpckilled")
	{
    //camerayaw(-80,3100);
    //wait(500);
    //cameraheight(2.3,10);
	  //cameradistance(7.6,10);
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
	  publishlogicevent("ge_area_clear", "game",1);
	  wait(2000);
		loop(10) 
		{ 
		  //检测网络状态 
		  while(!islobbyconnected() && @reconnectCount<10) 
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
		  if(islobbyconnected() && !islobbylogining()) 
		  { 
		    missioncompleted(0); 
		    wait(21000);
		    disconnectlobby(); 
		  } else {
		    wait(10000); 
		    //terminate(); 
		  };
		}; 
		changescene(0);
		terminate(); 
	};
  onmessage("missionfailed")
  {
    changescene(0);
    terminate();
  };
};
