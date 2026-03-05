using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions {
    [AddComponentMenu("Layout/Extensions/Radial Layout")]

    public class RadialLayoutGroup : LayoutGroup {
        public float fDistance;
        [Range(0f, 360f)]
        public float MinAngle, MaxAngle, StartAngle;

        // Mặc định nên để true để nó tự động bỏ qua object ẩn như các Layout Group chuẩn của Unity
        public bool OnlyLayoutVisible = true;

        protected override void OnEnable() { base.OnEnable(); CalculateRadial(); }

        public override void SetLayoutHorizontal() { }
        public override void SetLayoutVertical() { }

        public override void CalculateLayoutInputVertical() {
            CalculateRadial();
        }

        public override void CalculateLayoutInputHorizontal() {
            CalculateRadial();
        }

    #if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
            CalculateRadial();
        }
    #endif

        protected override void OnDisable() {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        void CalculateRadial() {
            m_Tracker.Clear();
            if (transform.childCount == 0)
                return;

            // BƯỚC 1: Lọc danh sách các child hợp lệ
            List<RectTransform> activeChildren = new List<RectTransform>();
            for (int i = 0; i < transform.childCount; i++) {
                RectTransform child = transform.GetChild(i) as RectTransform;
                if (child == null) continue;

                // Logic lọc: Nếu OnlyLayoutVisible bật -> chỉ lấy child đang active
                // Nếu tắt -> lấy hết (kể cả ẩn)
                if (!OnlyLayoutVisible || child.gameObject.activeSelf) {
                    activeChildren.Add(child);
                }
            }

            // BƯỚC 2: Kiểm tra số lượng để tránh lỗi chia cho 0
            int count = activeChildren.Count;
            if (count == 0) return;

            // BƯỚC 3: Tính toán góc chia
            // Tính góc chia dựa trên số lượng child thực tế đã lọc
            float fOffsetAngle = (MaxAngle - MinAngle) / count;
            float fAngle = StartAngle;

            // BƯỚC 4: Xếp vị trí
            for (int i = 0; i < count; i++) {
                RectTransform child = activeChildren[i];

                if (child != null) {
                    // Add vào tracker để Unity không cho phép chỉnh sửa vị trí bằng tay khi đang chạy layout
                    m_Tracker.Add(this, child,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.AnchoredPosition |
                    DrivenTransformProperties.Pivot);

                    // Tính toán vị trí vector
                    Vector3 vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0);
                    child.localPosition = vPos * fDistance;

                    // Force pivot và anchor về giữa để xoay cho chuẩn
                    child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);

                    // Cộng dồn góc cho phần tử tiếp theo
                    fAngle += fOffsetAngle;
                }
            }
        }
    }
}