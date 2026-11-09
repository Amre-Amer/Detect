import cv2
import pyvirtualcam
import socket
import json

# Setup UDP Socket for Unity communication
UDP_IP = "127.0.0.1"
UDP_PORT = 5005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Initialize webcam capture
cap = cv2.VideoCapture(0)
width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
fps = int(cap.get(cv2.CAP_PROP_FPS)) or 30

print(f"Webcam initialized: {width}x{height} @ {fps}FPS")

# Initialize pyvirtualcam (device="obs" targets the OBS Virtual Camera system)
with pyvirtualcam.Camera(width=width, height=height, fps=fps, device="obs", fmt=pyvirtualcam.PixelFormat.BGR) as cam:
    print(f"Virtual camera streaming as: {cam.device}")
    
    while True:
        ret, frame = cap.read()
        if not ret:
            break

        # --- PLACEHOLDER DETECTION LOGIC ---
        # Replace this with your actual model (YOLO, MediaPipe, etc.)
        # For demonstration, we create a fake box moving diagonally
        import time
        t = time.time()
        x, y, w, h = int((t * 50) % (width - 100)), int((t * 50) % (height - 100)), 100, 100
        
        # Draw detection on the frame (this will be seen in the restream)
        cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 255, 0), 2)
        cv2.putText(frame, "Detected Object", (x, y - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)
        # ------------------------------------

        # Send frame data to the Virtual Camera
        cam.send(frame)
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
