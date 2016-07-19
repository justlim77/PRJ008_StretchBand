<?php
// database settings
$isOnline 	= $_REQUEST['online'];
$action		= $_POST['action'];

if ($isOnline == "")
	$isOnline = 0;

//online
if ($isOnline == 1)
{
	$loginURL		="";
	$username		="";
	$password		="";
	$database		="";
}
else
{
	//offline
	$loginURL		="127.0.0.1:3306";
	$username		="root";
	$password		="";
	$database		="public";
}
