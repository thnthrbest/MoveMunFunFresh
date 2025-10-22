from ultralytics import YOLO
from deep_sort_realtime.deepsort_tracker import DeepSort
import cv2
import numpy as np
import socket
import struct
import threading

model = YOLO("yolov8m.pt")
model.fuse()

tracker = DeepSort(max_age=70, n_init=2, nms_max_overlap=0.8,
                   max_cosine_distance=0.4, nn_budget=100, max_iou_distance=0.7)


MAX_PEOPLE = 3  # ค่าเริ่มต้น
active_ids = set()
available_ids = list(range(1, MAX_PEOPLE + 1))
id_mapping = {}
smoothed_positions = {}
velocity_tracking = {}

MOTION_SMOOTHING = 0.6
VELOCITY_WEIGHT = 0.4

PROCESS_EVERY_N_FRAMES = 1
frame_count = 0
NowRun = True

#<---- unity send people ---->

def unity_listener():
    global MAX_PEOPLE
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.bind(('0.0.0.0', 5000))  # Python รอฟังที่พอร์ต 5000
    server.listen(1)
    print("Waiting for Unity connection on port 5000...")

    conn, addr = server.accept()
    print(f"Connected from {addr}")

    while True:
        data = conn.recv(1024)
        if not data:
            break
        try:
            MAX_PEOPLE = int(data.decode().strip())
            print(f"Received MAX_PEOPLE from Unity: {MAX_PEOPLE}")
        except:
            continue
    
    active_ids = set()
    available_ids = list(range(1, MAX_PEOPLE + 1))
    conn.close()

#<---- unity send people end---->


#<---- receive img unity ---->
threading.Thread(target=unity_listener, daemon=True).start()

tcp_server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
tcp_server.bind(("127.0.0.1", 5055))
tcp_server.listen(1)
print("Waiting for Unity connection...")
client_socket, client_address = tcp_server.accept()
print(f"Connected to {client_address}")

while True:
    # --- รับความยาวข้อมูลภาพก่อน ---
    length_data = client_socket.recv(4)
    if not length_data:
        break
    length = struct.unpack('I', length_data)[0]

    # --- รับข้อมูลภาพจริง ---
    image_data = b""
    while len(image_data) < length:
        image_data += client_socket.recv(length - len(image_data))
    np_arr = np.frombuffer(image_data, np.uint8)
    frame = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)
    frame = cv2.flip(frame, 1)

    frame_count += 1
    frame = cv2.convertScaleAbs(frame, alpha=1.1, beta=5)

    if frame_count % PROCESS_EVERY_N_FRAMES != 0:
        continue

    results = model(frame)

    for r in results:
        boxes = r.boxes.xyxy.cpu().numpy()
        confs = r.boxes.conf.cpu().numpy()
        clss = r.boxes.cls.cpu().numpy()

        detections = []
        for box, conf, cls in zip(boxes, confs, clss):
            if int(cls) == 0 and conf > 0.5:  # เฉพาะคน
                detections.append(([box[0], box[1], box[2]-box[0], box[3]-box[1]], conf, 'person'))

        tracks = tracker.update_tracks(detections, frame=frame)
        
        # เก็บ track_id ที่ยังมีอยู่ในเฟรมนี้
        current_track_ids = set()
        
        for track in tracks:
            if not track.is_confirmed():
                continue
            
            track_id = track.track_id
            current_track_ids.add(track_id)
            
            # ถ้ายังไม่มี custom_id สำหรับ track นี้
            if track_id not in id_mapping:
                # ตรวจสอบว่ายังมี ID ว่างไหม
                if available_ids:
                    custom_id = available_ids.pop(0)  # เอา ID ที่ว่างตัวแรก
                    id_mapping[track_id] = custom_id
                    active_ids.add(custom_id)
                else:
                    # ถ้า ID เต็มแล้ว ไม่แสดงคนนี้
                    continue
            
            custom_id = id_mapping[track_id]
            
            # วาดกรอบและแสดง custom_id
            x1, y1, x2, y2 = track.to_ltrb()
            cv2.rectangle(frame, (int(x1), int(y1)), (int(x2), int(y2)), (0, 255, 0), 2)
            cv2.putText(frame, f'ID {custom_id}', (int(x1), int(y1)-10),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2)
        
        # คืน ID ของคนที่หลุดจากเฟรม
        disappeared_tracks = set(id_mapping.keys()) - current_track_ids
        for disappeared_track_id in disappeared_tracks:
            custom_id = id_mapping[disappeared_track_id]
            available_ids.append(custom_id)  # คืน ID กลับไปใช้ใหม่
            available_ids.sort()  # เรียงเพื่อให้ใช้ ID เล็กที่สุดก่อน
            active_ids.discard(custom_id)
            del id_mapping[disappeared_track_id]

    _, img_encoded = cv2.imencode('.jpg', frame)
    data = img_encoded.tobytes()
    client_socket.send(struct.pack('I', len(data)))
    client_socket.send(data)

NowRun = False
client_socket.close()
tcp_server.close()
cv2.destroyAllWindows()
