﻿#
#IMPORTANT NOTE:
#  This file is generated by ProtoSql, DO NOT edit by hand.
#
#date: 2014/9/18 16:38:59
#author: Sirius
#
#version: 0.1.5
#version notes:
#  0.1.0
#    Initial version. Date:2014-09-09
#  0.1.1
#    玩家数据增加好友FriendInfo和伙伴PartnerInfo. Date:2014-09-10
#  0.1.2
#    邮件表MailInfo中的Text字段大小设为4096字节，数据类型改为text。 Date:2014-09-10
#  0.1.3
#    X魂表XSoulInfo中增加一列XSoulModelLevel。 Date:2014-09-12
#  0.1.4
#    AccountInfo表增加IsValid;UserInfoExtra表增加AttemptAward相关;ExpeditionInfo表增加Rage Date:2014-09-17
#  0.1.5
#    增加数据库版本验证机制。 Date:2014-09-18

use dsnode;
call SetDSNodeVersion('0.1.5');

create table Guid
(
  GuidTypeId int not null,
  IsValid boolean not null,
  GuidValue bigint not null,
  primary key (GuidTypeId)
) ENGINE=MyISAM;

create table Nickname
(
  Nickname char(255) not null,
  IsValid boolean not null,
  UserGuid bigint not null,
  primary key (Nickname)
) ENGINE=MyISAM;

create table GowStar
(
  Rank int not null,
  IsValid boolean not null,
  UserGuid bigint not null,
  Nickname char(255) not null,
  HeroId int not null,
  Level int not null,
  FightingScore int not null,
  GowElo int not null,
  primary key (Rank)
) ENGINE=MyISAM;

create table MailInfo
(
  Guid bigint not null,
  IsValid boolean not null,
  ModuleTypeId int not null,
  Sender char(255) not null,
  Receiver bigint not null,
  SendDate char(255) not null,
  ExpiryDate char(255) not null,
  Title char(255) not null,
  Text text(4096) not null,
  Money int not null,
  Gold int not null,
  Stamina int not null,
  ItemIds char(255) not null,
  ItemNumbers char(255) not null,
  LevelDemand int not null,
  IsRead boolean not null,
  primary key (Guid)
) ENGINE=MyISAM;

create table ActivationCode
(
  ActivationCode char(255) not null,
  IsValid boolean not null,
  IsActivated boolean not null,
  Account char(255) not null,
  primary key (ActivationCode)
) ENGINE=MyISAM;

create table Account
(
  Account char(255) not null,
  IsValid boolean not null,
  UserGuid1 bigint not null,
  UserGuid2 bigint not null,
  UserGuid3 bigint not null,
  primary key (Account)
) ENGINE=MyISAM;

create table UserInfo
(
  Guid bigint not null,
  AccountId char(255) not null,
  IsValid boolean not null,
  Nickname char(255) not null,
  HeroId int not null,
  Level int not null,
  Money int not null,
  Gold int not null,
  ExpPoints int not null,
  CitySceneId int not null,
  LastLogoutTime char(255) not null,
  CreateTime char(255) not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index UserInfoIndex on UserInfo (AccountId);

create table UserInfoExtra
(
  Guid bigint not null,
  IsValid boolean not null,
  GowElo int not null,
  GowMatches int not null,
  GowWinMatches int not null,
  Stamina int not null,
  BuyStaminaCount int not null,
  LastAddStaminaTimestamp double not null,
  BuyMoneyCount int not null,
  LastBuyMoneyTimestamp double not null,
  SellIncome int not null,
  LastSellTimestamp double not null,
  LastResetStaminaTime char(255) not null,
  LastResetMidasTouchTime char(255) not null,
  LastResetSellTime char(255) not null,
  LastResetDailyMissionTime char(255) not null,
  LastResetSceneCountTime char(255) not null,
  ActivePartnerId int not null,
  AttemptAward int not null,
  AttemptCurAcceptedCount int not null,
  AttemptAcceptedAward int not null,
  LastResetAttemptAwardCountTime char(255) not null,
  primary key (Guid)
) ENGINE=MyISAM;

create table EquipInfo
(
  Guid char(255) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  Position int not null,
  ItemId int not null,
  ItemNum int not null,
  Level int not null,
  AppendProperty int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index EquipInfoIndex on EquipInfo (UserGuid);

create table ItemInfo
(
  Guid char(255) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  Position int not null,
  ItemId int not null,
  ItemNum int not null,
  Level int not null,
  AppendProperty int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index ItemInfoIndex on ItemInfo (UserGuid);

create table LegacyInfo
(
  Guid char(255) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  Position int not null,
  LegacyId int not null,
  LegacyNum int not null,
  Level int not null,
  AppendProperty int not null,
  IsUnlock boolean not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index LegacyInfoIndex on LegacyInfo (UserGuid);

create table XSoulInfo
(
  Guid char(255) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  Position int not null,
  XSoulType int not null,
  XSoulId int not null,
  XSoulLevel int not null,
  XSoulExp int not null,
  XSoulModelLevel int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index XSoulInfoIndex on XSoulInfo (UserGuid);

create table SkillInfo
(
  Guid char(255) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  SkillId int not null,
  Level int not null,
  Preset int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index SkillInfoIndex on SkillInfo (UserGuid);

create table MissionInfo
(
  Guid char(255) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  MissionId int not null,
  MissionValue int not null,
  MissionState int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index MissionInfoIndex on MissionInfo (UserGuid);

create table LevelInfo
(
  Guid char(255) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  LevelId int not null,
  LevelRecord int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index LevelInfoIndex on LevelInfo (UserGuid);

create table ExpeditionInfo
(
  Guid char(255) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  StartTime double not null,
  FightingScore int not null,
  HP int not null,
  MP int not null,
  Rage int not null,
  Schedule int not null,
  MonsterCount int not null,
  BossCount int not null,
  OnePlayerCount int not null,
  Unrewarded char(255) not null,
  TollgateType int not null,
  EnemyList char(255) not null,
  EnemyAttrList char(255) not null,
  ImageA blob(32768) not null,
  ImageB blob(32768) not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index ExpeditionInfoIndex on ExpeditionInfo (UserGuid);

create table MailStateInfo
(
  Guid char(255) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  MailGuid bigint not null,
  IsRead boolean not null,
  IsReceived boolean not null,
  ExpiryDate char(255) not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index MailStateInfoIndex on MailStateInfo (UserGuid);

create table PartnerInfo
(
  Guid char(255) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  PartnerId int not null,
  AdditionLevel int not null,
  SkillLevel int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index PartnerInfoIndex on PartnerInfo (UserGuid);

create table FriendInfo
(
  Guid char(255) not null,
  UserGuid bigint not null,
  IsValid boolean not null,
  FriendGuid bigint not null,
  FriendNickname char(255) not null,
  HeroId int not null,
  Level int not null,
  FightingScore int not null,
  primary key (Guid)
) ENGINE=MyISAM;
create index FriendInfoIndex on FriendInfo (UserGuid);
