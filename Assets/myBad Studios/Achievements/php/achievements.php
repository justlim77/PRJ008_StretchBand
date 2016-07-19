<?php
include_once("config.php");

mysql_connect($loginURL,$username,$password);
@mysql_select_db($database) or die( "9");

$token_key				= "encryptionSalt";
$result					= "998:Invalid request";

$token					= mysql_real_escape_string( strip_tags($_POST['token']));
$key					= mysql_real_escape_string( strip_tags($_POST['key']));

//validate the data was sent from the game and not from a random user
//for this to work, the $token_key value in here must match the secret value set in Unity
if (MD5($token.$token_key) != $key)
{
	echo "999:Data origin verification failed";
	exit;
}


if (IsSet($action))
	switch($action)
	{
		case 0: CreateAchievement();	break;
		case 1: AwardAchievement();	break;
		case 2: GetAchievements();	break;
	}

mysql_close();
echo $result;

function CreateAchievement()
{
	global $result;
	
	$gid		= mysql_real_escape_string( strip_tags($_POST['gid']));
	$name		= mysql_real_escape_string( strip_tags($_POST['name']));
	$descr		= mysql_real_escape_string( strip_tags($_POST['descr']));
	$req		= mysql_real_escape_string( strip_tags($_POST['req']));
	$icongr		= mysql_real_escape_string( strip_tags($_POST['icongr']));
	$iconcol	= mysql_real_escape_string( strip_tags($_POST['iconcol']));
	
	$query		= "INSERT INTO achievables (gid, name, descr, req, icongr, iconcol) "
				. "VALUES ('$gid','$name','$descr','$req','$icongr','$iconcol')";
	mysql_query($query);

	$aid 		= mysql_insert_id();
	$query		= "INSERT INTO achievements (uid, aid, gid) VALUES ('0','$aid','$gid')";
	mysql_query($query);

	$result = "0:$aid";
}

function AwardAchievement()
{
	global $result;
	
	$gid		= mysql_real_escape_string( strip_tags($_POST['gid']));
	$uid		= mysql_real_escape_string( strip_tags($_POST['uid']));
	$aid		= mysql_real_escape_string( strip_tags($_POST['aid']));
	
	$query		= "SELECT aid FROM achievables WHERE gid = '$gid' AND aid = '$aid'";
	$achievement= mysql_query($query);
	$num 		= mysql_numrows($achievement);
	if ($num == 0)
	{
		$result = "2:Award does not exist";
		return;
	}
	
	$query		= "INSERT INTO achievements (uid, aid, gid) VALUES ('$uid','$aid','$gid')";
	mysql_query($query);

	$result = "0:Completed";
}

function GetAchievements()
{
	global $result;
	
	$gid		= mysql_real_escape_string( strip_tags($_POST['gid']));
	$uid		= mysql_real_escape_string( strip_tags($_POST['uid']));
	$subset		= mysql_real_escape_string( strip_tags($_POST['subset']));

	if (!IsSet($subset))
		$subset = "*";
	$query		= "SELECT $subset FROM achievables WHERE gid = '$gid' AND aid IN ("
				. "SELECT aid FROM achievements WHERE uid = '$uid' AND gid = '$gid')";
	$achieved	= mysql_query($query);
	$num 		= mysql_numrows($achieved);

	if ($num == 0)
	{
		$result = "1:No results found";
		return;
	}

	$result = "0";
	for($i = 0; $i < $num; $i++)
	{
		$thisrow = "";
		$data = mysql_fetch_assoc($achieved);
		foreach($data as $key => $val)
		{// $result .= "Key:$key => val:$val|";
			$thisentry = $key."=".$val;
			$thisRow .= ($thisRow == "") ? $thisentry : ",$thisentry";
		}
		$result	.= ":" . $thisRow;
	}
}
