import cv2
import numpy as np
from ultralytics import YOLO
import pyvirtualcam
import socket
import json

# Setup UDP Socket for Unity communication
UDP_IP = "127.0.0.1"
UDP_PORT = 5005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

model = YOLO('yolov8n.pt') 

# Initialize OpenCV webcam capture (0 is the default physical webcam)
cap = cv2.VideoCapture(0)

if not cap.isOpened():
    raise RuntimeError("Could not open physical webcam.")

# Get original width, height, and FPS
width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
fps = cap.get(cv2.CAP_PROP_FPS)
if fps <= 0:
    fps = 30.0

# Initialize pyvirtualcam targeting the OBS Virtual Camera backend
with pyvirtualcam.Camera(width=width, height=height, fps=fps, fmt=pyvirtualcam.PixelFormat.RGB) as cam:
    print(f"Streaming to virtual camera: {cam.device}")

    while True:
        ret, frame = cap.read(0)
        if not ret:
            break

        # --- PLACEHOLDER DETECTION LOGIC ---
        # Replace this with your actual model (YOLO, MediaPipe, etc.)
        # For demonstration, we create a fake box moving diagonally
        import time
        t = time.time()
        x, y, w, h = int((t * 50) % (width - 100)), int((t * 50) % (height - 100)), 100, 100
        
        # --- YOUR DETECTION / PROCESSING LOGIC HERE ---
        # Example: Drawing a simple label or bounding box placeholder
        cv2.putText(
            frame, 
            "Detection Active", 
            (50, 50), 
            cv2.FONT_HERSHEY_SIMPLEX, 
            1, 
            (0, 255, 0), 
            2
        )

        # Run face inference
        results = model(frame, conf=0.4, verbose=False)[0]
        
        # Plot default bounding boxes and keypoints onto the frame
        frame = results.plot()

        # Convert OpenCV BGR format to RGB format required by pyvirtualcam
        frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

        # Send frame to OBS Virtual Camera
        cam.send(frame_rgb)
        cam.sleep_until_next_frame()

                # Format and send detection results to Unity
        detection_data = {
            "x": x,
            "y": y,
            "width": w,
            "height": h,
            "frame_width": width,
            "frame_height": height
        }
        message = json.dumps(detection_data).encode('utf-8')
        sock.sendto(message, (UDP_IP, UDP_PORT))

        # Local preview break out clause (press 'q' to exit)
        cv2.imshow('Python Local Preview', frame)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

cap.release()
cv2.destroyAllWindows()

