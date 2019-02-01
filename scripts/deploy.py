from time import sleep
from distutils.dir_util import copy_tree
import os
import stat
import shutil
import datetime
import subprocess
 
target = input()

def del_rw(action, name, exc):
    os.chmod(name, stat.S_IWRITE)
    os.remove(name)

def push(dir, origin):
	subprocess.call ("git clone " + origin +" tmp", shell=True)
	copy_tree(dir, "tmp")
	subprocess.call ("git add --all", shell=True, cwd="tmp")
	subprocess.call ("git commit -m \"Deploy "+str(datetime.datetime.now())+"\"", shell=True, cwd="tmp")
	subprocess.call ("git push --set-upstream origin master", shell=True, cwd="tmp")
	shutil.rmtree("tmp", onerror=del_rw)

if target == "lobby-server":
	push("lobby-server", "git@github.com:minda-games/lobbyserver.git")
if target == "game-server":
	push("game-server", "git@github.com:minda-games/gameserver.git")
if target == "chatbot":
	push("chatbot", "git@github.com:minda-games/chatbot.git")
if target == "tslib":
	push("tslib", "git@github.com:minda-games/tslib.git")
