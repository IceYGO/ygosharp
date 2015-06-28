##
# ocgcore dynamic link library
# https://github.com/Fluorohydride/ygopro/tree/master/ocgcore
##

cmake_minimum_required (VERSION 2.8.9)

project (ocgcore)

set (OCGCORE_DIR "ocgcore")

set (OCGCORE_SRC
  ${OCGCORE_DIR}/card.cpp
  ${OCGCORE_DIR}/card.h
  ${OCGCORE_DIR}/common.h
  ${OCGCORE_DIR}/duel.cpp
  ${OCGCORE_DIR}/duel.h
  ${OCGCORE_DIR}/effect.cpp
  ${OCGCORE_DIR}/effect.h
  ${OCGCORE_DIR}/effectset.h
  ${OCGCORE_DIR}/field.cpp
  ${OCGCORE_DIR}/field.h
  ${OCGCORE_DIR}/group.cpp
  ${OCGCORE_DIR}/group.h
  ${OCGCORE_DIR}/interpreter.cpp
  ${OCGCORE_DIR}/interpreter.h
  ${OCGCORE_DIR}/libcard.cpp
  ${OCGCORE_DIR}/libdebug.cpp
  ${OCGCORE_DIR}/libduel.cpp
  ${OCGCORE_DIR}/libeffect.cpp
  ${OCGCORE_DIR}/libgroup.cpp
  ${OCGCORE_DIR}/mem.cpp
  ${OCGCORE_DIR}/mtrandom.h
  ${OCGCORE_DIR}/ocgapi.cpp
  ${OCGCORE_DIR}/ocgapi.h
  ${OCGCORE_DIR}/operations.cpp
  ${OCGCORE_DIR}/playerop.cpp
  ${OCGCORE_DIR}/processor.cpp
  ${OCGCORE_DIR}/scriptlib.cpp
  ${OCGCORE_DIR}/scriptlib.h
)

set (LUA_DIR "lua-5.3.0")

set (LUA_SRC
  ${LUA_DIR}/lapi.c
  ${LUA_DIR}/lapi.h
  ${LUA_DIR}/lauxlib.c
  ${LUA_DIR}/lauxlib.h
  ${LUA_DIR}/lbaselib.c
  ${LUA_DIR}/lbitlib.c
  ${LUA_DIR}/lcode.c
  ${LUA_DIR}/lcode.h
  ${LUA_DIR}/lcorolib.c
  ${LUA_DIR}/lctype.c
  ${LUA_DIR}/lctype.h
  ${LUA_DIR}/ldblib.c
  ${LUA_DIR}/ldebug.c
  ${LUA_DIR}/ldebug.h
  ${LUA_DIR}/ldo.c
  ${LUA_DIR}/ldo.h
  ${LUA_DIR}/ldump.c
  ${LUA_DIR}/lfunc.c
  ${LUA_DIR}/lfunc.h
  ${LUA_DIR}/lgc.c
  ${LUA_DIR}/lgc.h
  ${LUA_DIR}/linit.c
  ${LUA_DIR}/liolib.c
  ${LUA_DIR}/llex.c
  ${LUA_DIR}/llex.h
  ${LUA_DIR}/llimits.h
  ${LUA_DIR}/lmathlib.c
  ${LUA_DIR}/lmem.c
  ${LUA_DIR}/lmem.h
  ${LUA_DIR}/loadlib.c
  ${LUA_DIR}/lobject.c
  ${LUA_DIR}/lobject.h
  ${LUA_DIR}/lopcodes.c
  ${LUA_DIR}/lopcodes.h
  ${LUA_DIR}/loslib.c
  ${LUA_DIR}/lparser.c
  ${LUA_DIR}/lparser.h
  ${LUA_DIR}/lprefix.h
  ${LUA_DIR}/lstate.c
  ${LUA_DIR}/lstate.h
  ${LUA_DIR}/lstring.c
  ${LUA_DIR}/lstring.h
  ${LUA_DIR}/lstrlib.c
  ${LUA_DIR}/ltable.c
  ${LUA_DIR}/ltable.h
  ${LUA_DIR}/ltablib.c
  ${LUA_DIR}/ltm.c
  ${LUA_DIR}/ltm.h
  ${LUA_DIR}/luaconf.h
  ${LUA_DIR}/lua.h
  ${LUA_DIR}/lualib.h
  ${LUA_DIR}/lundump.c
  ${LUA_DIR}/lundump.h
  ${LUA_DIR}/lutf8lib.c
  ${LUA_DIR}/lvm.c
  ${LUA_DIR}/lvm.h
  ${LUA_DIR}/lzio.c
  ${LUA_DIR}/lzio.h
)

if (UNIX)
  set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11")
endif()

include_directories (
  ${LUA_DIR}
)

add_library (ocgcore SHARED
  ${OCGCORE_SRC}
  ${LUA_SRC}
)
