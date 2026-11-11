import cv2
from ultralytics import YOLO
import pyvirtualcam
import socket
import json

# Setup UDP Socket for Unity communication
UDP_IP = "127.0.0.1"
UDP_PORT = 5005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# 1. Load specialized face detection model (yolov8n-face is fast and lightweight)
# model = YOLO('yolov8n-face.pt') 
model = YOLO('yolov8n.pt') 

# 2. Open your video source (change to 0 if reading from a physical webcam instead)
video_path = "people-walking-from-the-side.mp4" 
cap = cv2.VideoCapture(video_path)

width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
fps = int(cap.get(cv2.CAP_PROP_FPS)) or 30

# 3. Initialize Virtual Camera for OBS to ingest
with pyvirtualcam.Camera(
    width=width, 
    height=height, 
    fps=fps, 
    backend="obs",
    device="OBS Virtual Camera",
    # delay_dict=None
    ) as cam:
    print(f"Virtual Camera streaming tracking data: {cam.device}")

    while cap.isOpened():
        success, frame = cap.read()
        if not success:
            cap.set(cv2.CAP_PROP_POS_FRAMES, 0)
            continue
            # break
            
        # --- PLACEHOLDER DETECTION LOGIC ---
        # Replace this with your actual model (YOLO, MediaPipe, etc.)
        # For demonstration, we create a fake box moving diagonally
        # import time
        # t = time.time()
        # x, y, w, h = int((t * 50) % (width - 100)), int((t * 50) % (height - 100)), 100, 100
        
        # Run face inference
        results = model(frame, conf=0.4, verbose=False)[0]
        
        # Plot default bounding boxes and keypoints onto the frame
        annotated_frame = results.plot()
        
        # PyVirtualCam requires RGB, OpenCV operates in BGR
        send_frame = cv2.cvtColor(annotated_frame, cv2.COLOR_BGR2RGB)
        
        # Send to Virtual Camera
        cam.send(send_frame)
        cam.sleep_until_next_frame()

        for r in results:
            boxes_data = []
            
            # Iterate through all detected objects in this frame
            for box in r.boxes:
                # Get coordinates in pixel units [xmin, ymin, xmax, ymax]
                # .tolist()[0] flattens the tensor values into a normal Python list
                xyxy = box.xyxy[0].cpu().tolist()
                
                # Alternate coordinate options:
                # xywh = box.xywh[0].cpu().tolist() # [x_center, y_center, width, height]

                confidence = float(box.conf[0].cpu())
                class_id = int(box.cls[0].cpu())
                class_name = model.names[class_id]

                # Format individual detection info
                detection = {
                    "class_name": class_name,
                    "class_id": class_id,
                    "confidence": round(confidence, 2),
                    "bbox": {
                        "xmin": int(xyxy[0]),
                        "ymin": int(xyxy[1]),
                        "xmax": int(xyxy[2]),
                        "ymax": int(xyxy[3])
                    }
                }
                boxes_data.append(detection)

            # Only transmit data if an object was detected
            if boxes_data:
                # Wrap data into a frame payload
                payload = {
                    "timestamp": cv2.getTickCount(), # Helpful tracker for ordering frames
                    "detections": boxes_data
                }

                # Serialize dictionary to a compact JSON string and encode to bytes
                json_data = json.dumps(payload).encode('utf-8')
                # message = json.dumps(detection_data).encode('utf-8')
        sock.sendto(json_data, (UDP_IP, UDP_PORT))

        # Local preview break out clause (press 'q' to exit)
        cv2.imshow('Python Local Preview', frame)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break


cap.release()
cv2.destroyAllWindows()
