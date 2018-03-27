import viz
import vizact
import vizmat
import socket
import sys
import threading 
from xml.dom import minidom
import math

# Generate IDs for the custom events that are triggered by this module.
CONNECTED_EVENT = viz.getEventID('CONNECTED_EVENT')
DISCONNECTED_EVENT = viz.getEventID('DISCONNECTED_EVENT')
CALIBRATION_FILE_GENERATED_EVENT = viz.getEventID('CALIBRATION_FILE_GENERATED_EVENT')
SCALAR_VALUE_RECEIVED_EVENT = viz.getEventID('SCALAR_VALUE_RECEIVED_EVENT')
VECTOR_VALUE_RECEIVED_EVENT = viz.getEventID('VECTOR_VALUE_RECEIVED_EVENT')
QUAT_VALUE_RECEIVED_EVENT = viz.getEventID('QUAT_VALUE_RECEIVED_EVENT')

# This is the maximum number of objects that we'll allow to be put into the scene.
MAX_OBJECTS = 4000;

# This is the name of the calibration file we will generate.
#CALIBRATION_FILE_NAME = 'calibration.acd'

# Create a global variable to hold the IP port.
ipPort = int(0)

# Create a list for holding the objects.
objs = []

# Create dictionaries for holding the output variable states.
scalars = {}
vectors = {}
quats = {}

def sendScalarValue(scalarName, scalarValue):

	# Declare globals.
	global scalars
	
	# Update the dictionary with the new value.
	scalars[scalarName] = scalarValue

def sendVectorValue(vectorName, vectorValue):

	# Declare globals.
	global vectors
	
	# Update the dictionary with the new value.
	vectors[vectorName] = vectorValue

def sendQuatValue(quatName, quatValue):

	# Declare globals.
	global quats
	
	# Update the dictionary with the new value.
	quats[quatName] = quatValue

def sendFrame(conn):
	
	# Declare globals.
	global scalars
	global vectors
	global quats
	
	# Assemble the output frame.
	msg = '<frame>\n'
	for name, scalar in scalars.items():
		msg += '<var name=\"'
		msg += name
		msg += '\" val=\"'
		msg += str(scalar)
		msg += '\">\n</var>\n'
	for name, vector in vectors.items():
		msg += '<var name=\"'
		msg += name
		msg += '\" x=\"'
		msg += str(vector.getX())
		msg += '\" y=\"'
		msg += str(vector.getY())
		msg += '\" z=\"'
		msg += str(vector.getZ())
		msg += '\">\n</var>\n'
	for name, quat in quats.items():
		msg += '<var name=\"'
		msg += name
		msg += '\" qw=\"'
		msg += str(quat.getW())
		msg += '\" qx=\"'
		msg += str(quat.getX())
		msg += '\" qy=\"'
		msg += str(quat.getY())
		msg += '\" qz=\"'
		msg += str(quat.getZ())
		msg += '\">\n</var>\n'
	msg += '</frame>'
	
	# Send the output frame.
	msglen = len(msg)
	totalsent = 0
	while totalsent < msglen:
		sent = conn.send(msg[totalsent:])
		if sent == 0:
			raise RuntimeError("socket connection broken")
		totalsent = totalsent + sent
		
# fileCreated = FALSE # moved from line 164 03/03/15 SSC per email from BEL on 02/19/15
# Create a function dedicated to handling TCP/IP communications.  This will run in a separate thread, since it contains blocking calls.
def serverProc():

	# Declare globals.
	global ipPort
	global objs

	# Initialize the variables that will be used to receive calibration information.
	eyeheight = 1.0
	eyeheightReceived = False	
	head = vizmat.Transform() # back-of-head position and gagnon orientation, both relative to original head sensor
	headposReceived = False
	headoriReceived = False
	lhand = vizmat.Transform() # left wrist position and gagnon orientation, both relative to original left hand sensor
	lhandposReceived = False
	lhandoriReceived = False
	lhandlength = 1.0
	lhandlengthReceived = False
	rhand = vizmat.Transform() # right wrist position and gagnon orientation, both relative to original right hand sensor
	rhandposReceived = False
	rhandoriReceived = False
	rhandlength = 1.0
	rhandlengthReceived = False
	lfoot = vizmat.Transform() # left ankle position and gagnon orientation, both relative to original left foot sensor
	lfootposReceived = False
	lfootoriReceived = False
	lfootlength = 1.0
	lfootlengthReceived = False
	rfoot = vizmat.Transform() # right ankle position and gagnon orientation, both relative to original right foot sensor
	rfootposReceived = False
	rfootoriReceived = False
	rfootlength = 1.0
	rfootlengthReceived = False
	pelvis = vizmat.Transform() # L5S1 position and gagnon orientation, both relative to original sacrum sensor
	pelvisposReceived = False
	pelvisoriReceived = False	
	lupperarm = vizmat.Transform() # left shoulder position and gagnon orientation, both relative to original left upper arm sensor
	lupperarmposReceived = False
	lupperarmoriReceived = False
	lupperarmlength = 1.0
	lupperarmlengthReceived = False
	rupperarm = vizmat.Transform() # right shoulder position and gagnon orientation, both relative to original right upper arm sensor
	rupperarmposReceived = False
	rupperarmoriReceived = False
	rupperarmlength = 1.0
	rupperarmlengthReceived = False
	lcalf = vizmat.Transform() # left knee position and gagnon orientation, both relative to original left shank sensor
	lcalfposReceived = False
	lcalforiReceived = False
	lcalflength = 1.0
	lcalflengthReceived = False
	rcalf = vizmat.Transform() # right knee position and gagnon orientation, both relative to original right shank sensor
	rcalfposReceived = False
	rcalforiReceived = False	
	rcalflength = 1.0
	rcalflengthReceived = False
	
	lforearmlength = 1.0  # BEL 3/6/15 - added these 8 lines
	lforearmlengthReceived = False
	rforearmlength = 1.0
	rforearmlengthReceived = False
	lthighlength = 1.0
	lthighlengthReceived = False
	rthighlength = 1.0
	rthighlengthReceived = False
	fileCreated = False # 03/03/15 SSC moved to line 107 per email from BEL on 02/19/15

	# Create a socket.
	sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

	# Associate the socket with a port.
	host = '' # this can be left blank on the server side
	sock.bind((host, ipPort))

	# Wait for the client to initiate a connection.
	sock.listen(1)
	conn, addr = sock.accept() # conn is a new socket, addr is the client's address

	# Indicate that a connection was made.
	viz.sendEvent(CONNECTED_EVENT)

	# Create a buffer to store the received data packets.
	inbuf = ''

	# Continuously read packets from the client, until they disconnect.
	while(1):

		# Wait for the next packet of data (up to 4096 bytes).  By default, this call will block until new data is available.
		data = conn.recv(4096)

		# If this is the end of data (meaning the client disconnected), break from loop.
		if not data:
			break

		# Add the new data to the buffer.
		inbuf += data

		# Process the records in the buffer until there are no more left.
		while (1):

			# 
			addobj_index = inbuf.find('</addobj>')
			remobj_index = inbuf.find('</remobj>')
			updobj_index = inbuf.find('</updobj>')
			updvar_index = inbuf.find('</updvar>')
			begstr_index = inbuf.find('</begstr>')
			endstr_index = inbuf.find('</endstr>')

			# Break from the loop if there are no more records.
			if (addobj_index == -1) and (remobj_index == -1) and (updobj_index == -1) and (updvar_index == -1) and (begstr_index == -1) and (endstr_index == -1):
				break;

			# Calculate the length of the first record in the buffer.
			if (addobj_index == -1):
				addobj_recordlength = 1000000
			else:
				addobj_recordlength = addobj_index + len('</addobj>')
			if (remobj_index == -1):
				remobj_recordlength = 1000000
			else:
				remobj_recordlength = remobj_index + len('</remobj>')
			if (updobj_index == -1):
				updobj_recordlength = 1000000
			else:
				updobj_recordlength = updobj_index + len('</updobj>')
			if (updvar_index == -1):
				updvar_recordlength = 1000000
			else:
				updvar_recordlength = updvar_index + len('</updvar>')
			if (begstr_index == -1):
				begstr_recordlength = 1000000
			else:
				begstr_recordlength = begstr_index + len('</begstr>')
			if (endstr_index == -1):
				endstr_recordlength = 1000000
			else:
				endstr_recordlength = endstr_index + len('</endstr>')
			recordlength = min(addobj_recordlength, remobj_recordlength, updobj_recordlength, updvar_recordlength, begstr_recordlength, endstr_recordlength)

			# Extract the record from the buffer.
			record = inbuf[0 : recordlength]

			# Delete the record from the buffer.
			inbuf = inbuf[recordlength : len(inbuf)]

			# 
			if (recordlength == addobj_recordlength):

				# 
				invalid = False

				# 
				indexstart = record.find(' i=\"')
				if (indexstart != -1):
					indexstart += len(' i=\"')
					indexend = record.find('\"', indexstart)
					index = int(record[indexstart : indexend])
					if (index < 0) or (index >= MAX_OBJECTS):
						continue
				else:
					continue

				# 
				while (len(objs) < index):
					objs.append([])

				# 
				modelstart = record.find(' model=\"')
				if (modelstart != -1):
					modelstart += len(' model=\"')
					modelend = record.find('\"', modelstart)
					model = record[modelstart : modelend]
				else:
					continue

				# 
				sxstart = record.find(' sx=\"')
				if (sxstart != -1):
					sxstart += len(' sx=\"')
					sxend = record.find('\"', sxstart)
					sx = float(record[sxstart : sxend])
				else:
					sx = 1.0

				# 
				systart = record.find(' sy=\"')
				if (systart != -1):
					systart += len(' sy=\"')
					syend = record.find('\"', systart)
					sy = float(record[systart : syend])
				else:
					sy = 1.0

				# 
				szstart = record.find(' sz=\"')
				if (szstart != -1):
					szstart += len(' sz=\"')
					szend = record.find('\"', szstart)
					sz = float(record[szstart : szend])
				else:
					sz = 1.0

				# 
				redstart = record.find(' red=\"')
				if (redstart != -1):
					redstart += len(' red=\"')
					redend = record.find('\"', redstart)
					red = float(record[redstart : redend])
				else:
					red = 0.0

				# 
				greenstart = record.find(' green=\"')
				if (greenstart != -1):
					greenstart += len(' green=\"')
					greenend = record.find('\"', greenstart)
					green = float(record[greenstart : greenend])
				else:
					green = 0.0

				# 
				bluestart = record.find(' blue=\"')
				if (bluestart != -1):
					bluestart += len(' blue=\"')
					blueend = record.find('\"', bluestart)
					blue = float(record[bluestart : blueend])
				else:
					blue = 0.0

				# 
				imagestart = record.find(' image=\"')
				if (imagestart != -1):
					imagestart += len(' image=\"')
					imageend = record.find('\"', imagestart)
					image = record[imagestart : imageend]
				else:
					image = ''

				# 
				if (image != ''):

					# 
					obj = viz.add(model, scale=[sx,sy,sz], cache=viz.CACHE_NONE)
					map = viz.add(image)
					obj.texture(map)

				# 
				else:

					# 
					obj = viz.add(model, color=[red,green,blue], scale=[sx,sy,sz], cache=viz.CACHE_NONE)

				# Place the object away from view.
				obj.setPosition([0, 1000, -1000])

				# The object's collision envelope will be the bounding sphere of the ball.
				obj.collideSphere(density=1000) # give the object a very high density, so that nothing else pushes it around

				# Manually negate the force of gravity.
				radius = obj.getBoundingSphere().radius
				obj.applyForce([0, (4.0 / 3.0) * math.pi * radius * radius * radius * 1000 * 9.8 * 0.189, 0], 1000000)

				# 
				obj.visible(viz.OFF)

				# Add the object to the list.
				if (index == len(objs)):
					objs.append([obj])
				else:
					objs[index] = [obj]

				# 
				xstart = record.find(' x=\"')
				if (xstart != -1):
					xstart += len(' x=\"')
					xend = record.find('\"', xstart)
					if (record[xstart : xend] == 'NaN'):
						invalid = True
					x = float(record[xstart : xend])
				else:
					x = None

				# 
				ystart = record.find(' y=\"')
				if (ystart != -1):
					ystart += len(' y=\"')
					yend = record.find('\"', ystart)
					if (record[ystart : yend] == 'NaN'):
						invalid = True
					y = float(record[ystart : yend])
				else:
					y = None

				# 
				zstart = record.find(' z=\"')
				if (zstart != -1):
					zstart += len(' z=\"')
					zend = record.find('\"', zstart)
					if (record[zstart : zend] == 'NaN'):
						invalid = True
					z = float(record[zstart : zend])
				else:
					z = None

				# 
				qwstart = record.find(' qw=\"')
				if (qwstart != -1):
					qwstart += len(' qw=\"')
					qwend = record.find('\"', qwstart)
					if (record[qwstart : qwend] == 'NaN'):
						invalid = True
					qw = float(record[qwstart : qwend])
				else:
					qw = None

				# 
				qxstart = record.find(' qx=\"')
				if (qxstart != -1):
					qxstart += len(' qx=\"')
					qxend = record.find('\"', qxstart)
					if (record[qxstart : qxend] == 'NaN'):
						invalid = True
					qx = float(record[qxstart : qxend])
				else:
					qx = None

				# 
				qystart = record.find(' qy=\"')
				if (qystart != -1):
					qystart += len(' qy=\"')
					qyend = record.find('\"', qystart)
					if (record[qystart : qyend] == 'NaN'):
						invalid = True
					qy = float(record[qystart : qyend])
				else:
					qy = None

				# 
				qzstart = record.find(' qz=\"')
				if (qzstart != -1):
					qzstart += len(' qz=\"')
					qzend = record.find('\"', qzstart)
					if (record[qzstart : qzend] == 'NaN'):
						invalid = True
					qz = float(record[qzstart : qzend])
				else:
					qz = None

				# 
				vxstart = record.find(' vx=\"')
				if (vxstart != -1):
					vxstart += len(' vx=\"')
					vxend = record.find('\"', vxstart)
					if (record[vxstart : vxend] == 'NaN'):
						invalid = True
					vx = float(record[vxstart : vxend])
				else:
					vx = None

				# 
				vystart = record.find(' vy=\"')
				if (vystart != -1):
					vystart += len(' vy=\"')
					vyend = record.find('\"', vystart)
					if (record[vystart : vyend] == 'NaN'):
						invalid = True
					vy = float(record[vystart : vyend])
				else:
					vy = None

				# 
				vzstart = record.find(' vz=\"')
				if (vzstart != -1):
					vzstart += len(' vz=\"')
					vzend = record.find('\"', vzstart)
					if (record[vzstart : vzend] == 'NaN'):
						invalid = True
					vz = float(record[vzstart : vzend])
				else:
					vz = None

				# 
				avxstart = record.find(' avx=\"')
				if (avxstart != -1):
					avxstart += len(' avx=\"')
					avxend = record.find('\"', avxstart)
					if (record[avxstart : avxend] == 'NaN'):
						invalid = True
					avx = float(record[avxstart : avxend])
				else:
					avx = None

				# 
				avystart = record.find(' avy=\"')
				if (avystart != -1):
					avystart += len(' avy=\"')
					avyend = record.find('\"', avystart)
					if (record[avystart : avyend] == 'NaN'):
						invalid = True
					avy = float(record[avystart : avyend])
				else:
					avy = None

				# 
				avzstart = record.find(' avz=\"')
				if (avzstart != -1):
					avzstart += len(' avz=\"')
					avzend = record.find('\"', avzstart)
					if (record[avzstart : avzend] == 'NaN'):
						invalid = True
					avz = float(record[avzstart : avzend])
				else:
					avz = None

				# 
				ostart = record.find(' o=\"')
				if (ostart != -1):
					ostart += len(' o=\"')
					oend = record.find('\"', ostart)
					if (record[ostart : oend] == 'NaN'):
						invalid = True
					opacity = float(record[ostart : oend])
				else:
					opacity = None

				# 
				if (invalid):
					continue

				# Disable dynamics if the object's velocity is zero (to prevent drift).
				if (((vx == 0) and (vy == 0) and (vz == 0)) or (vx == None) or (vy == None) or (vz == None)) and (((avx == 0) and (avy == 0) and (avz == 0)) or (avx == None) or (avy == None) or (avz == None)):
					objs[index][0].disable(viz.DYNAMICS) # unfortunately, this prevents us from giving the object a velocity!!!

				# Set the object's position.
				if (x != None) and (y != None) and (z != None):
					objs[index][0].setPosition([- y, z, x]) # update the position based on the rearranged world axes (see demo.py, line 35)

				# Set the object's orientation.
				if (qw != None) and (qx != None) and (qy != None) and (qz != None):
					objs[index][0].setQuat([qy, - qz, - qx, qw]) # update the orientation based on the rearranged world axes (see demo.py, line 35)

				# Set the object's opacity.
				if (opacity == 0.0):
					objs[index][0].visible(viz.OFF)
				else:
					objs[index][0].visible(viz.ON)

				# Set the object's velocity.
				if (vx != None) and (vy != None) and (vz != None):
					objs[index][0].setVelocity([- vy, vz, vx]) # update the velocity based on the rearranged world axes (see demo.py, line 35)

				# Set the object's angular velocity.
				if (avx != None) and (avy != None) and (avz != None):
					objs[index][0].setAngularVelocity([- avy, avz, avx]) # update the angular velocity based on the rearranged world axes (see demo.py, line 35)

				continue

			# 
			if (recordlength == remobj_recordlength):

				# 
				i = 0
				while (i < len(objs)):

					# 
					if objs[i] == []:
						i = i + 1
						continue

					# 
					objs[i][0].visible(viz.OFF)
					objs[i][0].setVelocity([0, 0, 0])
					objs[i][0].setAngularVelocity([0, 0, 0])
					objs[i][0].setPosition([0, 1000, -1000])

					# 
					i = i + 1

				# 
				objs = []

				continue

			# 
			if (recordlength == updobj_recordlength):

				# 
				invalid = False

				# 
				indexstart = record.find(' i=\"')
				if (indexstart != -1):
					indexstart += len(' i=\"')
					indexend = record.find('\"', indexstart)
					index = int(record[indexstart : indexend])
					if (index < 0) or (index >= len(objs)) or (objs[index] == []):
						continue
				else:
					continue

				# 
				xstart = record.find(' x=\"')
				if (xstart != -1):
					xstart += len(' x=\"')
					xend = record.find('\"', xstart)
					if (record[xstart : xend] == 'NaN'):
						invalid = True
					x = float(record[xstart : xend])
				else:
					x = None

				# 
				ystart = record.find(' y=\"')
				if (ystart != -1):
					ystart += len(' y=\"')
					yend = record.find('\"', ystart)
					if (record[ystart : yend] == 'NaN'):
						invalid = True
					y = float(record[ystart : yend])
				else:
					y = None

				# 
				zstart = record.find(' z=\"')
				if (zstart != -1):
					zstart += len(' z=\"')
					zend = record.find('\"', zstart)
					if (record[zstart : zend] == 'NaN'):
						invalid = True
					z = float(record[zstart : zend])
				else:
					z = None

				# 
				qwstart = record.find(' qw=\"')
				if (qwstart != -1):
					qwstart += len(' qw=\"')
					qwend = record.find('\"', qwstart)
					if (record[qwstart : qwend] == 'NaN'):
						invalid = True
					qw = float(record[qwstart : qwend])
				else:
					qw = None

				# 
				qxstart = record.find(' qx=\"')
				if (qxstart != -1):
					qxstart += len(' qx=\"')
					qxend = record.find('\"', qxstart)
					if (record[qxstart : qxend] == 'NaN'):
						invalid = True
					qx = float(record[qxstart : qxend])
				else:
					qx = None

				# 
				qystart = record.find(' qy=\"')
				if (qystart != -1):
					qystart += len(' qy=\"')
					qyend = record.find('\"', qystart)
					if (record[qystart : qyend] == 'NaN'):
						invalid = True
					qy = float(record[qystart : qyend])
				else:
					qy = None

				# 
				qzstart = record.find(' qz=\"')
				if (qzstart != -1):
					qzstart += len(' qz=\"')
					qzend = record.find('\"', qzstart)
					if (record[qzstart : qzend] == 'NaN'):
						invalid = True
					qz = float(record[qzstart : qzend])
				else:
					qz = None

				# 
				vxstart = record.find(' vx=\"')
				if (vxstart != -1):
					vxstart += len(' vx=\"')
					vxend = record.find('\"', vxstart)
					if (record[vxstart : vxend] == 'NaN'):
						invalid = True
					vx = float(record[vxstart : vxend])
				else:
					vx = None

				# 
				vystart = record.find(' vy=\"')
				if (vystart != -1):
					vystart += len(' vy=\"')
					vyend = record.find('\"', vystart)
					if (record[vystart : vyend] == 'NaN'):
						invalid = True
					vy = float(record[vystart : vyend])
				else:
					vy = None

				# 
				vzstart = record.find(' vz=\"')
				if (vzstart != -1):
					vzstart += len(' vz=\"')
					vzend = record.find('\"', vzstart)
					if (record[vzstart : vzend] == 'NaN'):
						invalid = True
					vz = float(record[vzstart : vzend])
				else:
					vz = None

				# 
				avxstart = record.find(' avx=\"')
				if (avxstart != -1):
					avxstart += len(' avx=\"')
					avxend = record.find('\"', avxstart)
					if (record[avxstart : avxend] == 'NaN'):
						invalid = True
					avx = float(record[avxstart : avxend])
				else:
					avx = None

				# 
				avystart = record.find(' avy=\"')
				if (avystart != -1):
					avystart += len(' avy=\"')
					avyend = record.find('\"', avystart)
					if (record[avystart : avyend] == 'NaN'):
						invalid = True
					avy = float(record[avystart : avyend])
				else:
					avy = None

				# 
				avzstart = record.find(' avz=\"')
				if (avzstart != -1):
					avzstart += len(' avz=\"')
					avzend = record.find('\"', avzstart)
					if (record[avzstart : avzend] == 'NaN'):
						invalid = True
					avz = float(record[avzstart : avzend])
				else:
					avz = None

				# 
				ostart = record.find(' o=\"')
				if (ostart != -1):
					ostart += len(' o=\"')
					oend = record.find('\"', ostart)
					if (record[ostart : oend] == 'NaN'):
						invalid = True
					opacity = float(record[ostart : oend])
				else:
					opacity = None

				# 
				if (invalid):
					objs[index][0].visible(viz.OFF)
					objs[index][0].setVelocity([0, 0, 0])
					objs[index][0].setAngularVelocity([0, 0, 0])
					objs[index][0].setPosition([0, 1000, -1000])
					continue

				# Disable dynamics if the object's velocity is zero (to prevent drift).
				if (vx != None) and (vy != None) and (vz != None):
					if (vx == 0) and (vy == 0) and (vz == 0) and (((avx == 0) and (avy == 0) and (avz == 0)) or (avx == None) or (avy == None) or (avz == None)):
						objs[index][0].disable(viz.DYNAMICS) # unfortunately, this prevents us from giving the object a velocity!!!
					else:
						objs[index][0].enable(viz.DYNAMICS)

				# Set the object's position.
				if (x != None) and (y != None) and (z != None):
					objs[index][0].setPosition([- y, z, x]) # update the position based on the rearranged world axes (see demo.py, line 35)

				# Set the object's orientation.
				if (qw != None) and (qx != None) and (qy != None) and (qz != None):
					objs[index][0].setQuat([qy, - qz, - qx, qw]) # update the orientation based on the rearranged world axes (see demo.py, line 35)

				# Set the object's opacity.
				if (opacity == 0.0):
					objs[index][0].visible(viz.OFF)
				else:
					objs[index][0].visible(viz.ON)

				# Set the object's velocity.
				if (vx != None) and (vy != None) and (vz != None):
					objs[index][0].setVelocity([- vy, vz, vx]) # update the velocity based on the rearranged world axes (see demo.py, line 35)

				# Set the object's angular velocity.
				if (avx != None) and (avy != None) and (avz != None):
					objs[index][0].setAngularVelocity([- avy, avz, avx]) # update the angular velocity based on the rearranged world axes (see demo.py, line 35)

				continue

			# 
			if (recordlength == updvar_recordlength):

				# 
				invalid = False

				# 
				namestart = record.find(' name=\"')
				if (namestart != -1):
					namestart += len(' name=\"')
					nameend = record.find('\"', namestart)
					name = record[namestart : nameend]
				else:
					continue

				# 
				valstart = record.find(' val=\"')
				if (valstart != -1):
					valstart += len(' val=\"')
					valend = record.find('\"', valstart)
					if (record[valstart : valend] == 'NaN'):
						invalid = True
					val = float(record[valstart : valend])
				else:
					val = None

				# 
				xstart = record.find(' x=\"')
				if (xstart != -1):
					xstart += len(' x=\"')
					xend = record.find('\"', xstart)
					if (record[xstart : xend] == 'NaN'):
						invalid = True
					x = float(record[xstart : xend])
				else:
					x = None

				# 
				ystart = record.find(' y=\"')
				if (ystart != -1):
					ystart += len(' y=\"')
					yend = record.find('\"', ystart)
					if (record[ystart : yend] == 'NaN'):
						invalid = True
					y = float(record[ystart : yend])
				else:
					y = None

				# 
				zstart = record.find(' z=\"')
				if (zstart != -1):
					zstart += len(' z=\"')
					zend = record.find('\"', zstart)
					if (record[zstart : zend] == 'NaN'):
						invalid = True
					z = float(record[zstart : zend])
				else:
					z = None

				# 
				qwstart = record.find(' qw=\"')
				if (qwstart != -1):
					qwstart += len(' qw=\"')
					qwend = record.find('\"', qwstart)
					if (record[qwstart : qwend] == 'NaN'):
						invalid = True
					qw = float(record[qwstart : qwend])
				else:
					qw = None

				# 
				qxstart = record.find(' qx=\"')
				if (qxstart != -1):
					qxstart += len(' qx=\"')
					qxend = record.find('\"', qxstart)
					if (record[qxstart : qxend] == 'NaN'):
						invalid = True
					qx = float(record[qxstart : qxend])
				else:
					qx = None

				# 
				qystart = record.find(' qy=\"')
				if (qystart != -1):
					qystart += len(' qy=\"')
					qyend = record.find('\"', qystart)
					if (record[qystart : qyend] == 'NaN'):
						invalid = True
					qy = float(record[qystart : qyend])
				else:
					qy = None

				# 
				qzstart = record.find(' qz=\"')
				if (qzstart != -1):
					qzstart += len(' qz=\"')
					qzend = record.find('\"', qzstart)
					if (record[qzstart : qzend] == 'NaN'):
						invalid = True
					qz = float(record[qzstart : qzend])
				else:
					qz = None

				# 
				if (val != None):
					viz.sendEvent(SCALAR_VALUE_RECEIVED_EVENT, name, val, invalid)
					
				# 
				if (x != None) and (y != None) and (z != None):
					viz.sendEvent(VECTOR_VALUE_RECEIVED_EVENT, name, vizmat.Vector(x, y, z), invalid)
					
				# 
				if (qx != None) and (qy != None) and (qz != None) and (qw != None):
					viz.sendEvent(QUAT_VALUE_RECEIVED_EVENT, name, vizmat.Quat(qx, qy, qz, qw), invalid)
					
				#
#				print 'received', name
#				print x,y,z
				if (name == 'eyeheight'):
					if (val != None) and not invalid:
						if (not eyeheightReceived):
							print 'eyeheight received'
						eyeheight = val
						eyeheightReceived = True
				if (name == 'headpos'):
					if (x != None) and (y != None) and (z != None) and not invalid:
						if (not headposReceived):
							print 'headpos received'
						head.setPosition(x, y, z)
						headposReceived = True
				if (name == 'headori'):
					if (qx != None) and (qy != None) and (qz != None) and (qw != None) and not invalid:
						if (not headoriReceived):
							print 'headori received'
						head.setQuat(qx, qy, qz, qw)
						headoriReceived = True
				if (name == 'lhandpos'):
					if (x != None) and (y != None) and (z != None) and not invalid:
						if (not lhandposReceived):
							print 'lhandpos received'
						lhand.setPosition(x, y, z)
						lhandposReceived = True
				if (name == 'lhandori'):
					if (qx != None) and (qy != None) and (qz != None) and (qw != None) and not invalid:
						if (not lhandoriReceived):
							print 'lhandori received'
						lhand.setQuat(qx, qy, qz, qw)
						lhandoriReceived = True
				if (name == 'lhandlength'):
					if (val != None) and not invalid:
						if (not lhandlengthReceived):
							print 'lhandlength received'
						lhandlength = val
						lhandlengthReceived = True
				if (name == 'rhandpos'):
					if (x != None) and (y != None) and (z != None) and not invalid:
						if (not rhandposReceived):
							print 'rhandpos received'
						rhand.setPosition(x, y, z)
						rhandposReceived = True
				if (name == 'rhandori'):
					if (qx != None) and (qy != None) and (qz != None) and (qw != None) and not invalid:
						if (not rhandoriReceived):
							print 'rhandori received'
						rhand.setQuat(qx, qy, qz, qw)
						rhandoriReceived = True
				if (name == 'rhandlength'):
					if (val != None) and not invalid:
						if (not rhandlengthReceived):
							print 'rhandlength received'
						rhandlength = val
						rhandlengthReceived = True
				if (name == 'lfootpos'):
					if (x != None) and (y != None) and (z != None) and not invalid:
						if (not lfootposReceived):
							print 'lfootpos received'
						lfoot.setPosition(x, y, z)
						lfootposReceived = True
				if (name == 'lfootori'):
					if (qx != None) and (qy != None) and (qz != None) and (qw != None) and not invalid:
						if (not lfootoriReceived):
							print 'lfootori received'
						lfoot.setQuat(qx, qy, qz, qw)
						lfootoriReceived = True
				if (name == 'lfootlength'):
					if (val != None) and not invalid:
						if (not lfootlengthReceived):
							print 'lfootlength received'
						lfootlength = val
						lfootlengthReceived = True
				if (name == 'rfootpos'):
					if (x != None) and (y != None) and (z != None) and not invalid:
						if (not rfootposReceived):
							print 'rfootpos received'
						rfoot.setPosition(x, y, z)
						rfootposReceived = True
				if (name == 'rfootori'):
					if (qx != None) and (qy != None) and (qz != None) and (qw != None) and not invalid:
						if (not rfootoriReceived):
							print 'rfootori received'
						rfoot.setQuat(qx, qy, qz, qw)
						rfootoriReceived = True
				if (name == 'rfootlength'):
					if (val != None) and not invalid:
						if (not rfootlengthReceived):
							print 'rfootlength received'
						rfootlength = val
						rfootlengthReceived = True
				if (name == 'pelvispos'):
					if (x != None) and (y != None) and (z != None) and not invalid:
						if (not pelvisposReceived):
							print 'pelvispos received'
						pelvis.setPosition(x, y, z)
						pelvisposReceived = True
				if (name == 'pelvisori'):
					if (qx != None) and (qy != None) and (qz != None) and (qw != None) and not invalid:
						if (not pelvisoriReceived):
							print 'pelvisori received'
						pelvis.setQuat(qx, qy, qz, qw)
						pelvisoriReceived = True
				if (name == 'lupperarmpos'):
					if (x != None) and (y != None) and (z != None) and not invalid:
						if (not lupperarmposReceived):
							print 'lupperarmpos received'
						lupperarm.setPosition(x, y, z)
						lupperarmposReceived = True
				if (name == 'lupperarmori'):
					if (qx != None) and (qy != None) and (qz != None) and (qw != None) and not invalid:
						if (not lupperarmoriReceived):
							print 'lupperarmori received'
						lupperarm.setQuat(qx, qy, qz, qw)
						lupperarmoriReceived = True
				if (name == 'lupperarmlength'):
					if (val != None) and not invalid:
						if (not lupperarmlengthReceived):
							print 'lupperarmlength received'
						lupperarmlength = val
						lupperarmlengthReceived = True
				if (name == 'rupperarmpos'):
					if (x != None) and (y != None) and (z != None) and not invalid:
						if (not rupperarmposReceived):
							print 'rupperarmpos received'
						rupperarm.setPosition(x, y, z)
						rupperarmposReceived = True
				if (name == 'rupperarmori'):
					if (qx != None) and (qy != None) and (qz != None) and (qw != None) and not invalid:
						if (not rupperarmoriReceived):
							print 'rupperarmori received'
						rupperarm.setQuat(qx, qy, qz, qw)
						rupperarmoriReceived = True
				if (name == 'rupperarmlength'):
					if (val != None) and not invalid:
						if (not rupperarmlengthReceived):
							print 'rupperarmlength received'
						rupperarmlength = val
						rupperarmlengthReceived = True
				if (name == 'lcalfpos'):
					if (x != None) and (y != None) and (z != None) and not invalid:
						if (not lcalfposReceived):
							print 'lcalfpos received'
						lcalf.setPosition(x, y, z)
						lcalfposReceived = True
				if (name == 'lcalfori'):
					if (qx != None) and (qy != None) and (qz != None) and (qw != None) and not invalid:
						if (not lcalforiReceived):
							print 'lcalfori received'
						lcalf.setQuat(qx, qy, qz, qw)
						lcalforiReceived = True
				if (name == 'lcalflength'):
					if (val != None) and not invalid:
						if (not lcalflengthReceived):
							print 'lcalflength received'
						lcalflength = val
						lcalflengthReceived = True
				if (name == 'rcalfpos'):
					if (x != None) and (y != None) and (z != None) and not invalid:
						if (not rcalfposReceived):
							print 'rcalfpos received'
						rcalf.setPosition(x, y, z)
						rcalfposReceived = True
				if (name == 'rcalfori'):
					if (qx != None) and (qy != None) and (qz != None) and (qw != None) and not invalid:
						if (not rcalforiReceived):
							print 'rcalfori received'
						rcalf.setQuat(qx, qy, qz, qw)
						rcalforiReceived = True
				if (name == 'rcalflength'):
					if (val != None) and not invalid:
						if (not rcalflengthReceived):
							print 'rcalflength received'
						rcalflength = val
						rcalflengthReceived = True
				if (name == 'lforearmlength'):  # BEL 3/6/15 - added these 24 lines
					if (val != None) and not invalid:
						if (not lforearmlengthReceived):
							print 'lforearmlength received'
						lforearmlength = val
						lforearmlengthReceived = True
				if (name == 'rforearmlength'):
					if (val != None) and not invalid:
						if (not rforearmlengthReceived):
							print 'rforearmlength received'
						rforearmlength = val
						rforearmlengthReceived = True
				if (name == 'lthighlength'):
					if (val != None) and not invalid:
						if (not lthighlengthReceived):
							print 'lthighlength received'
						lthighlength = val
						lthighlengthReceived = True
				if (name == 'rthighlength'):
					if (val != None) and not invalid:
						if (not rthighlengthReceived):
							print 'rthighlength received'
						rthighlength = val
						rthighlengthReceived = True


				continue

			# 
			if (recordlength == begstr_recordlength):

				# 
				ratestart = record.find(' rate=\"')
				if (ratestart != -1):
					ratestart += len(' rate=\"')
					rateend = record.find('\"', ratestart)
					rate = float(record[ratestart : rateend])
				else:
					rate = 100.0

				# 
				timerAction = vizact.ontimer(1.0 / rate, sendFrame, conn)

				continue

			# 
			if (recordlength == endstr_recordlength):

				# 
				timerAction.setEnabled(0)

				continue

		if ((not fileCreated) and
		eyeheightReceived and		
		headposReceived and
		headoriReceived and
		lhandposReceived and
		lhandoriReceived and
		lhandlengthReceived and
		rhandposReceived and
		rhandoriReceived and
		rhandlengthReceived and
		lfootposReceived and
		lfootoriReceived and
		lfootlengthReceived and
		rfootposReceived and
		rfootoriReceived and
		rfootlengthReceived and
		pelvisposReceived and
		pelvisoriReceived and
		lupperarmposReceived and
		lupperarmoriReceived and
		lupperarmlengthReceived and
		rupperarmposReceived and
		rupperarmoriReceived and
		rupperarmlengthReceived and
		lcalfposReceived and
		lcalforiReceived and
		lcalflengthReceived and
		rcalfposReceived and
		rcalforiReceived and
		rcalflengthReceived and
		lforearmlengthReceived and  # BEL 3/6/15 - added these 4 lines
		rforearmlengthReceived and
		lthighlengthReceived and
		rthighlengthReceived):

#		lhandlength = 0.289
#		rhandlength = 0.289
			
			
			"""
			# This section of code calculates and prints the constants needed to generate the ACD file, below.  Enable it whenever you need to re-calculate these constants.
			
			# The following values came from the ACD file generated by the WorldViz demo script (they are specific to the Tracker data files being streamed)
			_scale = 1.020304525167797
			_lfoot = vizmat.Transform(0.999519, 0.030825, 0.003322, 0.000000, -0.030849, 0.999496, 0.007428, 0.000000, -0.003092, -0.007527, 0.999967, 0.000000, 0.077058, 0.010830, -0.108658, 1.000000)
			_head = vizmat.Transform(1.000000, 0.000000, 0.000000, 0.000000, 0.000000, 1.000000, 0.000000, 0.000000, 0.000000, 0.000000, 1.000000, 0.000000, 0.000000, 0.000000, 0.000000, 1.000000)
			_rupperarm = vizmat.Transform(0.096086, -0.987414, 0.125621, 0.000000, 0.429525, 0.154981, 0.889657, 0.000000, -0.897929, -0.031526, 0.439011, 0.000000, -0.000161, 0.163449, 0.027728, 1.000000)
			_rcalf = vizmat.Transform(0.999909, 0.010847, -0.007984, 0.000000, -0.010818, 0.999935, 0.003682, 0.000000, 0.008023, -0.003595, 0.999961, 0.000000, -0.098273, 0.138762, -0.009101, 1.000000)
			_rfoot = vizmat.Transform(0.999960, 0.006170, -0.006452, 0.000000, -0.006196, 0.999972, -0.004081, 0.000000, 0.006427, 0.004121, 0.999971, 0.000000, -0.099829, 0.010458, -0.131004, 1.000000)
			_rhand = vizmat.Transform(0.928634, -0.262206, -0.262462, 0.000000, 0.337818, 0.305204, 0.890354, 0.000000, -0.153352, -0.915477, 0.372000, 0.000000, 0.008188, -0.045050, 0.013508, 1.000000)
			_lhand = vizmat.Transform(0.961459, 0.210289, 0.177129, 0.000000, -0.238137, 0.314894, 0.918767, 0.000000, 0.137429, -0.925538, 0.352836, 0.000000, -0.017774, -0.054941, -0.018841, 1.000000)
			_lupperarm = vizmat.Transform(0.073747, 0.996958, -0.025225, 0.000000, -0.324813, 0.047927, 0.944563, 0.000000, 0.942899, -0.061466, 0.327359, 0.000000, -0.014742, 0.188803, 0.072922, 1.000000)
			_pelvis = vizmat.Transform(0.999178, 0.004073, -0.040328, 0.000000, -0.004885, 0.999787, -0.020047, 0.000000, 0.040238, 0.020228, 0.998985, 0.000000, 0.011266, -0.153723, 0.055996, 1.000000)
			_lcalf = vizmat.Transform(0.999963, 0.006699, 0.005352, 0.000000, -0.006639, 0.999914, -0.011277, 0.000000, -0.005427, 0.011241, 0.999922, 0.000000, 0.118467, 0.172576, -0.019441, 1.000000)

# 			'boneLengthDict': {'l_foot': 0.43070965260267385, 'head': 0.10509780694388951, 'r_upper_arm': 0.14830944032104523, 'r_calf': 0.39769876003265436, 'r_foot': 0.43070952594280293, 'l_clavicle': 0.04125190884664276, 'l_hand': 0.2892541289329529, 'r_clavicle': 0.04125193369097894, 'r_hand': 0.289254054427147, 'r_forearm': 0.28652334958317377, 'l_thigh': 0.16175531555022182, 'pelvis': 0.0, 'l_forearm': 0.28652334213259356, 'l_upper_arm': 0.1482830575453593, 'l_calf': 0.39769858121872087, 'torso': 0.15709042549133936, 'r_thigh': 0.16175592420974153}

			# Calculate the head constants, by reversing the calculations that generate the head entry in the ACD file.
			headpos = head.getPosition()
			newheadpos = vizmat.Vector(- headpos[1], headpos[2], headpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			headori = head.getQuat()
			newheadori = vizmat.Quat(headori[1], - headori[2], - headori[0], headori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newhead = vizmat.Transform()
			newhead.setPosition(newheadpos)
			newhead.setQuat(newheadori)
			t0 = _head * newhead.inverse()
			print 'head'
			v0 = vizmat.Vector(t0.getPosition())
			v0 /= _scale
			print v0
			print t0.getQuat()

			# Calculate the lhand constants, by reversing the calculations that generate the lhand entry in the ACD file.
			lhandpos = lhand.getPosition()
			newlhandpos = vizmat.Vector(- lhandpos[1], lhandpos[2], lhandpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			lhandori = lhand.getQuat()
			newlhandori = vizmat.Quat(lhandori[1], - lhandori[2], - lhandori[0], lhandori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newlhand = vizmat.Transform()
			newlhand.setPosition(newlhandpos)
			newlhand.setQuat(newlhandori)
			t1 = _lhand * newlhand.inverse()
			print 'lhand'
			v1 = vizmat.Vector(t1.getPosition())
			v1 /= _scale
			print v1
			print t1.getQuat()

			# Calculate the rhand constants, by reversing the calculations that generate the rhand entry in the ACD file.
			rhandpos = rhand.getPosition()
			newrhandpos = vizmat.Vector(- rhandpos[1], rhandpos[2], rhandpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			rhandori = rhand.getQuat()
			newrhandori = vizmat.Quat(rhandori[1], - rhandori[2], - rhandori[0], rhandori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newrhand = vizmat.Transform()
			newrhand.setPosition(newrhandpos)
			newrhand.setQuat(newrhandori)
			t2 = _rhand * newrhand.inverse()
			print 'rhand'
			v2 = vizmat.Vector(t2.getPosition())
			v2 /= _scale
			print v2
			print t2.getQuat()

			# Calculate the lfoot constants, by reversing the calculations that generate the lfoot entry in the ACD file.
			lfootpos = lfoot.getPosition()
			newlfootpos = vizmat.Vector(- lfootpos[1], lfootpos[2], lfootpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			lfootori = lfoot.getQuat()
			newlfootori = vizmat.Quat(lfootori[1], - lfootori[2], - lfootori[0], lfootori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newlfoot = vizmat.Transform()
			newlfoot.setPosition(newlfootpos)
			newlfoot.setQuat(newlfootori)
			t3 = _lfoot * newlfoot.inverse()
			print 'lfoot'
			v3 = vizmat.Vector(t3.getPosition())
			v3 /= _scale
			print v3
			print t3.getQuat()

			# Calculate the rfoot constants, by reversing the calculations that generate the rfoot entry in the ACD file.
			rfootpos = rfoot.getPosition()
			newrfootpos = vizmat.Vector(- rfootpos[1], rfootpos[2], rfootpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			rfootori = rfoot.getQuat()
			newrfootori = vizmat.Quat(rfootori[1], - rfootori[2], - rfootori[0], rfootori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newrfoot = vizmat.Transform()
			newrfoot.setPosition(newrfootpos)
			newrfoot.setQuat(newrfootori)
			t4 = _rfoot * newrfoot.inverse()
			print 'rfoot'
			v4 = vizmat.Vector(t4.getPosition())
			v4 /= _scale
			print v4
			print t4.getQuat()

			# Calculate the pelvis constants, by reversing the calculations that generate the pelvis entry in the ACD file.
			pelvispos = pelvis.getPosition()
			newpelvispos = vizmat.Vector(- pelvispos[1], pelvispos[2], pelvispos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			pelvisori = pelvis.getQuat()
			newpelvisori = vizmat.Quat(pelvisori[1], - pelvisori[2], - pelvisori[0], pelvisori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newpelvis = vizmat.Transform()
			newpelvis.setPosition(newpelvispos)
			newpelvis.setQuat(newpelvisori)
			t5 = _pelvis * newpelvis.inverse()
			print 'pelvis'
			v5 = vizmat.Vector(t5.getPosition())
			v5 /= _scale
			print v5
			print t5.getQuat()

			# Calculate the lupperarm constants, by reversing the calculations that generate the lupperarm entry in the ACD file.
			lupperarmpos = lupperarm.getPosition()
			newlupperarmpos = vizmat.Vector(- lupperarmpos[1], lupperarmpos[2], lupperarmpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			lupperarmori = lupperarm.getQuat()
			newlupperarmori = vizmat.Quat(lupperarmori[1], - lupperarmori[2], - lupperarmori[0], lupperarmori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newlupperarm = vizmat.Transform()
			newlupperarm.setPosition(newlupperarmpos)
			newlupperarm.setQuat(newlupperarmori)
			t6 = _lupperarm * newlupperarm.inverse()
			print 'lupperarm'
			v6 = vizmat.Vector(t6.getPosition())
			v6 /= _scale
			print v6
			print t6.getQuat()

			# Calculate the rupperarm constants, by reversing the calculations that generate the rupperarm entry in the ACD file.
			rupperarmpos = rupperarm.getPosition()
			newrupperarmpos = vizmat.Vector(- rupperarmpos[1], rupperarmpos[2], rupperarmpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			rupperarmori = rupperarm.getQuat()
			newrupperarmori = vizmat.Quat(rupperarmori[1], - rupperarmori[2], - rupperarmori[0], rupperarmori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newrupperarm = vizmat.Transform()
			newrupperarm.setPosition(newrupperarmpos)
			newrupperarm.setQuat(newrupperarmori)
			t7 = _rupperarm * newrupperarm.inverse()
			print 'rupperarm'
			v7 = vizmat.Vector(t7.getPosition())
			v7 /= _scale
			print v7
			print t7.getQuat()

			# Calculate the lcalf constants, by reversing the calculations that generate the lcalf entry in the ACD file.
			lcalfpos = lcalf.getPosition()
			newlcalfpos = vizmat.Vector(- lcalfpos[1], lcalfpos[2], lcalfpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			lcalfori = lcalf.getQuat()
			newlcalfori = vizmat.Quat(lcalfori[1], - lcalfori[2], - lcalfori[0], lcalfori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newlcalf = vizmat.Transform()
			newlcalf.setPosition(newlcalfpos)
			newlcalf.setQuat(newlcalfori)
			t8 = _lcalf * newlcalf.inverse()
			print 'lcalf'
			v8 = vizmat.Vector(t8.getPosition())
			v8 /= _scale
			print v8
			print t8.getQuat()

			# Calculate the rcalf constants, by reversing the calculations that generate the rcalf entry in the ACD file.
			rcalfpos = rcalf.getPosition()
			newrcalfpos = vizmat.Vector(- rcalfpos[1], rcalfpos[2], rcalfpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			rcalfori = rcalf.getQuat()
			newrcalfori = vizmat.Quat(rcalfori[1], - rcalfori[2], - rcalfori[0], rcalfori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newrcalf = vizmat.Transform()
			newrcalf.setPosition(newrcalfpos)
			newrcalf.setQuat(newrcalfori)
			t9 = _rcalf * newrcalf.inverse()
			print 'rcalf'
			v9 = vizmat.Vector(t9.getPosition())
			v9 /= _scale
			print v9
			print t9.getQuat()

			# ===== End of code which calculates the ACD file constants =====
			"""
			

			
			scale = eyeheight * (1.020304525167797 / 1.496025)
			
			headpos = head.getPosition()
			newheadpos = vizmat.Vector(- headpos[1], headpos[2], headpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			headori = head.getQuat()
			newheadori = vizmat.Quat(headori[1], - headori[2], - headori[0], headori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newhead = vizmat.Transform()
			newhead.setPosition(newheadpos)
			newhead.setQuat(newheadori)
#			mat0 = vizmat.Transform()
#			v0 = vizmat.Vector(0.02379409207807195, 0.04756500741637771, 0.07703691891111866) # offset vector from back-of-head (calculated in previous section)
#			v0 *= scale
#			mat0.setPosition(v0)
#			mat0.setQuat(0.03366504018742267, 0.7900704117131814, 0.6120849122907275, 0.002733084447250063) # offset quaternion from rearranged Gagnon orientation (calculated in previous section)
			mat0 = vizmat.Transform(-1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0.06, 0, 1) # REPLACES THE FIVE LINES ABOVE... MANUALLY DERIVED, WHERE THE BASE REFERENCE FRAME IS Z LONGITUDINAL, Y ANTERIOR, and X RIGHTWARD
			mat0.postMult(newhead)

			lhandpos = lhand.getPosition()
			newlhandpos = vizmat.Vector(- lhandpos[1], lhandpos[2], lhandpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			lhandori = lhand.getQuat()
			newlhandori = vizmat.Quat(lhandori[1], - lhandori[2], - lhandori[0], lhandori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newlhand = vizmat.Transform()
			newlhand.setPosition(newlhandpos)
			newlhand.setQuat(newlhandori)
#			mat1 = vizmat.Transform()
#			v1 = vizmat.Vector(0.05084983232795847, -0.01955418109398445, 0.06128938843102665) # offset vector from wrist (calculated in previous section)
#			v1 *= scale
#			mat1.setPosition(v1)
#			mat1.setQuat(0.015453231719691391, -0.05721231849793863, 0.9981659962033462, -0.012352824166544058) # offset quaternion from rearranged Gagnon orientation (calculated in previous section)
			mat1 = vizmat.Transform(-1, 0, 0, 0, 0, -1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1) # REPLACES THE FIVE LINES ABOVE... MANUALLY DERIVED, WHERE THE BASE REFERENCE FRAME IS Z LONGITUDINAL, Y ANTERIOR, and X RIGHTWARD
			mat1.postMult(newlhand)
			
			rhandpos = rhand.getPosition()
			newrhandpos = vizmat.Vector(- rhandpos[1], rhandpos[2], rhandpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			rhandori = rhand.getQuat()
			newrhandori = vizmat.Quat(rhandori[1], - rhandori[2], - rhandori[0], rhandori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newrhand = vizmat.Transform()
			newrhand.setPosition(newrhandpos)
			newrhand.setQuat(newrhandori)
#			mat2 = vizmat.Transform()
#			v2 = vizmat.Vector(-0.02369108576354897, -0.025475628701251883, 0.0822584495042945) # offset vector from wrist (calculated in previous section)
#			v2 *= scale
#			mat2.setPosition(v2)
#			mat2.setQuat(0.04652552992781981, 0.03694071839647062, 0.9980044147471597, -0.021399685382441035) # offset quaternion from rearranged Gagnon orientation (calculated in previous section)
			mat2 = vizmat.Transform(-1, 0, 0, 0, 0, -1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1) # REPLACES THE FIVE LINES ABOVE... MANUALLY DERIVED, WHERE THE BASE REFERENCE FRAME IS Z LONGITUDINAL, Y ANTERIOR, and X RIGHTWARD
			mat2.postMult(newrhand)
			
			lfootpos = lfoot.getPosition()
			newlfootpos = vizmat.Vector(- lfootpos[1], lfootpos[2], lfootpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			lfootori = lfoot.getQuat()
			newlfootori = vizmat.Quat(lfootori[1], - lfootori[2], - lfootori[0], lfootori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newlfoot = vizmat.Transform()
			newlfoot.setPosition(newlfootpos)
			newlfoot.setQuat(newlfootori)
#			mat3 = vizmat.Transform()
#			v3 = vizmat.Vector(0.0398742821651038, -0.0406719455785957, -0.06851088239762072) # offset vector from ankle (calculated in previous section)
#			v3 *= scale
#			mat3.setPosition(v3)
#			mat3.setQuat(-0.12489926296036119, -0.008218288369850876, 0.07962209494777515, 0.9889352637277913) # offset quaternion from rearranged Gagnon orientation (calculated in previous section)
			mat3 = vizmat.Transform(1, 0, 0, 0, 0, 0.9239, -0.3827, 0, 0, 0.3827, 0.9239, 0, 0, 0, 0, 1) # REPLACES THE FIVE LINES ABOVE... MANUALLY DERIVED, WHERE THE BASE REFERENCE FRAME IS Z LONGITUDINAL, Y ANTERIOR, and X RIGHTWARD
			mat3.postMult(newlfoot)

			rfootpos = rfoot.getPosition()
			newrfootpos = vizmat.Vector(- rfootpos[1], rfootpos[2], rfootpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			rfootori = rfoot.getQuat()
			newrfootori = vizmat.Quat(rfootori[1], - rfootori[2], - rfootori[0], rfootori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newrfoot = vizmat.Transform()
			newrfoot.setPosition(newrfootpos)
			newrfoot.setQuat(newrfootori)
#			mat4 = vizmat.Transform()
#			v4 = vizmat.Vector(-0.0706185990907538, -0.07586649661656979, -0.05678419196750229) # offset vector from ankle (calculated in previous section)
#			v4 *= scale
#			mat4.setPosition(v4)
#			mat4.setQuat(-0.20359633256248127, -0.21655580749830802, 0.04370204978449001, 0.9538040922802216) # offset quaternion from rearranged Gagnon orientation (calculated in previous section)
			mat4 = vizmat.Transform(1, 0, 0, 0, 0, 0.9239, -0.3827, 0, 0, 0.3827, 0.9239, 0, 0, 0, 0, 1) # REPLACES THE FIVE LINES ABOVE... MANUALLY DERIVED, WHERE THE BASE REFERENCE FRAME IS Z LONGITUDINAL, Y ANTERIOR, and X RIGHTWARD
			mat4.postMult(newrfoot)

			pelvispos = pelvis.getPosition()
			newpelvispos = vizmat.Vector(- pelvispos[1], pelvispos[2], pelvispos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			pelvisori = pelvis.getQuat()
			newpelvisori = vizmat.Quat(pelvisori[1], - pelvisori[2], - pelvisori[0], pelvisori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newpelvis = vizmat.Transform()
			newpelvis.setPosition(newpelvispos)
			newpelvis.setQuat(newpelvisori)
#			mat5 = vizmat.Transform()
#			v5 = vizmat.Vector(0.02025559749471931, -0.03236167732041706, 0.10767530829972147) # offset vector from L5S1 (calculated in previous section)
#			v5 *= scale
#			mat5.setPosition(v5)
#			mat5.setQuat(-0.7495111013875013, 0.011200546076094392, -0.002565202298745491, 0.6618920428602072) # offset quaternion from rearranged Gagnon orientation (calculated in previous section)
			mat5 = vizmat.Transform(1, 0, 0, 0, 0, 0, -1, 0, 0, 1, 0, 0, 0, 0, 0, 1) # REPLACES THE FIVE LINES ABOVE... MANUALLY DERIVED, WHERE THE BASE REFERENCE FRAME IS Z LONGITUDINAL, Y ANTERIOR, and X RIGHTWARD
			mat5.postMult(newpelvis)

			lupperarmpos = lupperarm.getPosition()
			newlupperarmpos = vizmat.Vector(- lupperarmpos[1], lupperarmpos[2], lupperarmpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			lupperarmori = lupperarm.getQuat()
			newlupperarmori = vizmat.Quat(lupperarmori[1], - lupperarmori[2], - lupperarmori[0], lupperarmori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newlupperarm = vizmat.Transform()
			newlupperarm.setPosition(newlupperarmpos)
			newlupperarm.setQuat(newlupperarmori)
#			mat6 = vizmat.Transform()
#			v6 = vizmat.Vector(-0.0979971080034664, -0.07239435999782545, 0.04624169891140371) # offset vector from wrist (calculated in previous section)
#			v6 *= scale
#			mat6.setPosition(v6)
#			mat6.setQuat(-0.42395404198950193, 0.57352122944369, 0.5024974500313925, 0.48870510777984544) # offset quaternion from rearranged Gagnon orientation (calculated in previous section)
			mat6 = vizmat.Transform(0, 0, -1, 0, -1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1) # REPLACES THE FIVE LINES ABOVE... MANUALLY DERIVED, WHERE THE BASE REFERENCE FRAME IS Z LONGITUDINAL, Y ANTERIOR, and X RIGHTWARD
			mat6.postMult(newlupperarm)
			
			rupperarmpos = rupperarm.getPosition()
			newrupperarmpos = vizmat.Vector(- rupperarmpos[1], rupperarmpos[2], rupperarmpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			rupperarmori = rupperarm.getQuat()
			newrupperarmori = vizmat.Quat(rupperarmori[1], - rupperarmori[2], - rupperarmori[0], rupperarmori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newrupperarm = vizmat.Transform()
			newrupperarm.setPosition(newrupperarmpos)
			newrupperarm.setQuat(newrupperarmori)
#			mat7 = vizmat.Transform()
#			v7 = vizmat.Vector(0.09303898755436728, -0.06071769563296285, 0.0644047906928154) # offset vector from wrist (calculated in previous section)
#			v7 *= scale
#			mat7.setPosition(v7)
#			mat7.setQuat(0.44674728127309965, 0.5553554334497675, 0.5009660685116167, -0.4909482736632537) # offset quaternion from rearranged Gagnon orientation (calculated in previous section)
			mat7 = vizmat.Transform(0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1) # REPLACES THE FIVE LINES ABOVE... MANUALLY DERIVED, WHERE THE BASE REFERENCE FRAME IS Z LONGITUDINAL, Y ANTERIOR, and X RIGHTWARD
			mat7.postMult(newrupperarm)
			
			lcalfpos = lcalf.getPosition()
			newlcalfpos = vizmat.Vector(- lcalfpos[1], lcalfpos[2], lcalfpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			lcalfori = lcalf.getQuat()
			newlcalfori = vizmat.Quat(lcalfori[1], - lcalfori[2], - lcalfori[0], lcalfori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newlcalf = vizmat.Transform()
			newlcalf.setPosition(newlcalfpos)
			newlcalf.setQuat(newlcalfori)
#			mat8 = vizmat.Transform()
#			v8 = vizmat.Vector(0.0232732495967431, -0.04405727822921606, 0.003073654180567417) # offset vector from ankle (calculated in previous section)
#			v8 *= scale
#			mat8.setPosition(v8)
#			mat8.setQuat(-0.7154342397206898, 0.01699725800006888, 0.04694277211799108, 0.6968940507721645) # offset quaternion from rearranged Gagnon orientation (calculated in previous section)
			mat8 = vizmat.Transform(1, 0, 0, 0, 0, 0, -1, 0, 0, 1, 0, 0, 0, 0, 0, 1) # REPLACES THE FIVE LINES ABOVE... MANUALLY DERIVED, WHERE THE BASE REFERENCE FRAME IS Z LONGITUDINAL, Y ANTERIOR, and X RIGHTWARD
			mat8.postMult(newlcalf)

			rcalfpos = rcalf.getPosition()
			newrcalfpos = vizmat.Vector(- rcalfpos[1], rcalfpos[2], rcalfpos[0]) # update the offset vector based on the sensor's rearranged axes (see swapQuat() in demo.py)
			rcalfori = rcalf.getQuat()
			newrcalfori = vizmat.Quat(rcalfori[1], - rcalfori[2], - rcalfori[0], rcalfori[3]) # update the offset quaternion based on the sensor's rearranged axes (see swapQuat() in demo.py)
			newrcalf = vizmat.Transform()
			newrcalf.setPosition(newrcalfpos)
			newrcalf.setQuat(newrcalfori)
#			mat9 = vizmat.Transform()
#			v9 = vizmat.Vector(-0.0176002729240911, -0.03448381228787304, 0.014766453663458697) # offset vector from ankle (calculated in previous section)
#			v9 *= scale
#			mat9.setPosition(v9)
#			mat9.setQuat(-0.7286629205839205, -0.02595051924489143, -0.022831779086659766, 0.683999728494661) # offset quaternion from rearranged Gagnon orientation (calculated in previous section)
			mat9 = vizmat.Transform(1, 0, 0, 0, 0, 0, -1, 0, 0, 1, 0, 0, 0, 0, 0, 1) # REPLACES THE FIVE LINES ABOVE... MANUALLY DERIVED, WHERE THE BASE REFERENCE FRAME IS Z LONGITUDINAL, Y ANTERIOR, and X RIGHTWARD
			mat9.postMult(newrcalf)

			fileCreated = True

			viz.postEvent(CALIBRATION_FILE_GENERATED_EVENT, eyeheight, mat0, mat1, lhandlength, mat2, rhandlength, mat3, lfootlength, mat4, rfootlength, mat5, mat6, lupperarmlength, mat7, rupperarmlength, mat8, lcalflength, mat9, rcalflength, lforearmlength, rforearmlength, lthighlength, rthighlength)  # BEL 3/6/15 - added last 4 args

	# Close the socket.
	conn.close()

	# Indicate that the client disconnected.
	viz.sendEvent(DISCONNECTED_EVENT)

def waitForConnection(port):

	# Declare globals.
	global ipPort
	
	# Save the IP port number in the global variable.
	ipPort = port
	
	# Run the communications routine in a separate thread.
	threading.Thread(target = serverProc).start()
