-- phpMyAdmin SQL Dump
-- version 3.2.4
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Mar 16, 2012 at 11:42 PM
-- Server version: 5.1.44
-- PHP Version: 5.3.1

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

CREATE TABLE IF NOT EXISTS achievables (
  gid int(10) unsigned NOT NULL,
  aid int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL DEFAULT '',
  descr varchar(128) NOT NULL DEFAULT '',
  req varchar(256) NOT NULL DEFAULT '',
  icongr varchar(50) NOT NULL DEFAULT 'blank.png',
  iconcol varchar(50) NOT NULL DEFAULT 'blank.png',
  PRIMARY KEY (gid,aid)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COMMENT='List of all achievements available' AUTO_INCREMENT=1 ;

CREATE TABLE IF NOT EXISTS achievements (
  gid int(10) unsigned NOT NULL DEFAULT '0',
  uid varchar(32) NOT NULL DEFAULT '',
  aid int(11) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (gid,uid,aid)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
