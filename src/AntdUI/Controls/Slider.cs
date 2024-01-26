﻿// COPYRIGHT (C) Tom. ALL RIGHTS RESERVED.
// THE AntdUI PROJECT IS AN WINFORM LIBRARY LICENSED UNDER THE GPL-3.0 License.
// LICENSED UNDER THE GPL License, VERSION 3.0 (THE "License")
// YOU MAY NOT USE THIS FILE EXCEPT IN COMPLIANCE WITH THE License.
// UNLESS REQUIRED BY APPLICABLE LAW OR AGREED TO IN WRITING, SOFTWARE
// DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED.
// SEE THE LICENSE FOR THE SPECIFIC LANGUAGE GOVERNING PERMISSIONS AND
// LIMITATIONS UNDER THE License.
// GITEE: https://gitee.com/antdui/AntdUI
// GITHUB: https://github.com/AntdUI/AntdUI
// CSDN: https://blog.csdn.net/v_132
// QQ: 17379620

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI
{
    /// <summary>
    /// Slider 滑动输入条
    /// </summary>
    /// <remarks>滑动型输入器，展示当前值和可选范围。</remarks>
    [Description("Slider 滑动输入条")]
    [ToolboxItem(true)]
    [DefaultProperty("Value")]
    [DefaultEvent("ValueChanged")]
    public class Slider : IControl
    {
        #region 属性

        /// <summary>
        /// 固定点
        /// </summary>
        [Description("固定点"), Category("数据"), DefaultValue(null)]
        public int[]? Dots { get; set; }

        Color? fill;
        /// <summary>
        /// 颜色
        /// </summary>
        [Description("颜色"), Category("外观"), DefaultValue(null)]
        public Color? Fill
        {
            get { return fill; }
            set
            {
                if (fill == value) return;
                fill = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 悬停颜色
        /// </summary>
        [Description("悬停颜色"), Category("外观"), DefaultValue(null)]
        public Color? FillHover { get; set; }

        /// <summary>
        /// 激活颜色
        /// </summary>
        [Description("激活颜色"), Category("外观"), DefaultValue(null)]
        public Color? FillActive { get; set; }

        int _minValue = 0;
        /// <summary>
        /// 最小值
        /// </summary>
        [Description("最小值"), Category("数据"), DefaultValue(0)]
        public int MinValue
        {
            get { return _minValue; }
            set
            {
                if (value > _maxValue) return;
                if (_minValue == value) return;
                _minValue = value;
                Invalidate();
            }
        }

        int _maxValue = 100;
        /// <summary>
        /// 最大值
        /// </summary>
        [Description("最大值"), Category("数据"), DefaultValue(100)]
        public int MaxValue
        {
            get { return _maxValue; }
            set
            {
                if (value < _minValue || value < _value) return;
                if (_maxValue == value) return;
                _maxValue = value;
                Invalidate();
            }
        }

        int _value = 0;
        /// <summary>
        /// 当前值
        /// </summary>
        [Description("当前值"), Category("数据"), DefaultValue(0)]
        public int Value
        {
            get { return _value; }
            set
            {
                if (value < _minValue) value = _minValue;
                else if (value > _maxValue) value = _maxValue;
                if (_value == value) return;
                _value = value;
                ValueChanged?.Invoke(this, _value);
                Invalidate();
            }
        }

        public delegate string ValueFormatEventHandler(int value);
        /// <summary>
        /// Value格式化时发生
        /// </summary>
        [Description("Value格式化时发生"), Category("行为")]
        public event ValueFormatEventHandler? ValueFormatChanged;

        TooltipForm? tooltipForm = null;
        string? tooltipText = null;
        void ShowTips(RectangleF dot_rect)
        {
            var text = ValueFormatChanged == null ? Value.ToString() : ValueFormatChanged.Invoke(Value);
            if (text == tooltipText && tooltipForm != null) return;
            tooltipText = text;
            var _rect = RectangleToScreen(ClientRectangle);
            var rect = new Rectangle(_rect.X + (int)dot_rect.X, _rect.Y + (int)dot_rect.Y, (int)dot_rect.Width, (int)dot_rect.Height);
            if (tooltipForm == null)
            {
                tooltipForm = new TooltipForm(rect, tooltipText, new TooltipConfig
                {
                    Font = Font,
                    ArrowAlign = vertical ? TAlign.Right : TAlign.Top,
                });
                tooltipForm.Show(this);
            }
            else
            {
                tooltipForm.SetText(rect, tooltipText);
            }
        }

        void CloseTips()
        {
            tooltipForm?.IClose();
            tooltipForm = null;
        }

        bool vertical = false;
        /// <summary>
        /// 是否垂直方向
        /// </summary>
        [Description("是否垂直方向"), Category("外观"), DefaultValue(false)]
        public bool Vertical
        {
            get => vertical;
            set
            {
                if (vertical == value) return;
                vertical = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Value 属性值更改时发生
        /// </summary>
        [Description("Value 属性值更改时发生"), Category("行为")]
        public event IntEventHandler? ValueChanged;

        /// <summary>
        /// 是否显示数值
        /// </summary>
        [Description("是否显示数值"), Category("行为"), DefaultValue(false)]
        public bool ShowValue { get; set; } = false;

        int lineSize = 4;
        /// <summary>
        /// 线条粗细
        /// </summary>
        [Description("线条粗细"), Category("外观"), DefaultValue(4)]
        public int LineSize
        {
            get { return lineSize; }
            set
            {
                if (lineSize == value) return;
                lineSize = value;
                Invalidate();
            }
        }

        int dotSize = 14;
        /// <summary>
        /// 点大小
        /// </summary>
        [Description("点大小"), Category("外观"), DefaultValue(14)]
        public int DotSize
        {
            get { return dotSize; }
            set
            {
                if (dotSize == value) return;
                dotSize = value;
                Invalidate();
            }
        }

        int dotSizeActive = 20;
        /// <summary>
        /// 点激活大小
        /// </summary>
        [Description("点激活大小"), Category("外观"), DefaultValue(20)]
        public int DotSizeActive
        {
            get { return dotSizeActive; }
            set
            {
                if (dotSizeActive == value) return;
                dotSizeActive = value;
                Invalidate();
            }
        }

        #endregion

        #region 渲染

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics.High();
            var _rect = ClientRectangle;

            var rect = CRectF(_rect);
            var back = Style.Db.FillQuaternary;
            using (var brush = new SolidBrush(back))
            {
                g.FillRectangle(brush, rect);

                if (AnimationHover)
                {
                    using (var brush2 = new SolidBrush(Color.FromArgb((int)(back.A * AnimationHoverValue), back)))
                    {
                        g.FillRectangle(brush2, rect);
                    }
                }
                else if (ExtraMouseHover)
                {
                    g.FillRectangle(brush, rect);
                }
            }
            float prog = CProg(_value, rect.Width, rect.Height);
            if (_value > 0)
            {
                RectangleF rect_prog;
                if (vertical) rect_prog = new RectangleF(rect.X, rect.Y, rect.Width, prog);
                else rect_prog = new RectangleF(rect.X, rect.Y, prog, rect.Height);

                Color color = fill.HasValue ? fill.Value : Style.Db.InfoBorder, color_hover = FillHover.HasValue ? FillHover.Value : Style.Db.InfoHover;
                if (AnimationHover)
                {
                    using (var brush = new SolidBrush(color))
                    {
                        g.FillRectangle(brush, rect_prog);
                    }
                    using (var brush = new SolidBrush(Color.FromArgb((int)(255 * AnimationHoverValue), color_hover)))
                    {
                        g.FillRectangle(brush, rect_prog);
                    }
                }
                else
                {
                    using (var brush = new SolidBrush(ExtraMouseHover ? color_hover : color))
                    {
                        g.FillRectangle(brush, rect_prog);
                    }
                }
            }
            PaintEllipse(g, _rect, rect, prog);
            this.PaintBadge(g);
        }

        internal void PaintEllipse(Graphics g, Rectangle _rect, RectangleF rect, float prog)
        {
            var color = fill.HasValue ? fill.Value : Style.Db.InfoBorder;
            var color_active = FillActive.HasValue ? FillActive.Value : Style.Db.Primary;
            using (var brush = new SolidBrush(Style.Db.BgBase))
            {
                if (Dots != null && Dots.Length > 0)
                {
                    foreach (var it in Dots)
                    {
                        float size = dotSize * 0.9F;
                        float uks = CProg(it, rect.Width, rect.Height);
                        var rect_dot = CRect(_rect, rect, uks, size);
                        g.FillEllipse(brush, rect_dot);
                        PaintEllipse(g, rect_dot, color, 1);
                    }
                }
                var rect_ellipse_rl = CRect(_rect, rect, prog, dotSize);
                if (ShowValue && _mouseHover) ShowTips(rect_ellipse_rl);
                if (AnimationHover)
                {
                    int size2 = dotSizeActive - dotSize;
                    var size = dotSize + size2 * AnimationHoverValue;
                    var rect_ellipse = CRect(_rect, rect, prog, size);
                    g.FillEllipse(brush, rect_ellipse);
                    PaintEllipse(g, rect_ellipse, color_active, 2 + 2 * AnimationHoverValue);
                }
                else if (ExtraMouseHover)
                {
                    var rect_ellipse = CRect(_rect, rect, prog, dotSizeActive);
                    g.FillEllipse(brush, rect_ellipse);
                    PaintEllipse(g, rect_ellipse, color_active, 4);
                }
                else
                {
                    g.FillEllipse(brush, rect_ellipse_rl);
                    PaintEllipse(g, rect_ellipse_rl, color, 2);
                }
            }
        }

        #region 计算区域

        internal float CProg(int val, float w, float h)
        {
            if (val > 0)
            {
                if (vertical) return val >= _maxValue ? h : h * (val * 1F / _maxValue);
                else return val >= _maxValue ? w : w * (val * 1F / _maxValue);
            }
            return 0;
        }
        internal RectangleF CRect(Rectangle _rect, RectangleF rect, float prog, float size)
        {
            if (vertical) return new RectangleF(_rect.X + (_rect.Width - size) / 2F, rect.Y + prog - size / 2, size, size);
            else return new RectangleF(rect.X + prog - size / 2, _rect.Y + (_rect.Height - size) / 2F, size, size);
        }
        internal RectangleF CRectF(Rectangle _rect)
        {
            if (vertical) return new RectangleF(_rect.Left + (_rect.Width - lineSize) / 2F, dotSizeActive, lineSize, _rect.Height - dotSizeActive * 2);
            else return new RectangleF(dotSizeActive, _rect.Top + (_rect.Height - lineSize) / 2F, _rect.Width - dotSizeActive * 2, lineSize);
        }

        #endregion

        internal void PaintEllipse(Graphics g, RectangleF rect_ellipse, Color color, float size)
        {
            using (var brush = new Pen(color, size))
            {
                g.DrawEllipse(brush, rect_ellipse);
            }
        }

        #endregion

        #region 鼠标

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                mouseFlat = true;
                var _rect = ClientRectangle;
                var rect = CRectF(_rect);
                if (vertical)
                {
                    if (Dots != null && Dots.Length > 0)
                    {
                        foreach (var it in Dots)
                        {
                            float uks = CProg(it, rect.Width, rect.Height);
                            var rect_dot = new RectangleF(_rect.X, rect.Y + uks - dotSize / 2, _rect.Width, dotSize);
                            if (rect_dot.Contains(e.Location))
                            {
                                Value = it;
                                return;
                            }
                        }
                    }

                    float y = (e.Y - rect.Y) * 1.0F / rect.Height;
                    if (y > 0) Value = (int)Math.Round(y * _maxValue);
                    else Value = 0;
                }
                else
                {
                    if (Dots != null && Dots.Length > 0)
                    {
                        foreach (var it in Dots)
                        {
                            float uks = CProg(it, rect.Width, rect.Height);
                            var rect_dot = new RectangleF(rect.X + uks - dotSize / 2, _rect.Y, dotSize, _rect.Height);
                            if (rect_dot.Contains(e.Location))
                            {
                                Value = it;
                                return;
                            }
                        }
                    }
                    float x = (e.X - rect.X) * 1.0F / rect.Width;
                    if (x > 0) Value = (int)Math.Round(x * _maxValue);
                    else Value = 0;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (mouseFlat)
            {
                var _rect = ClientRectangle;
                var rect = CRectF(_rect);
                if (vertical)
                {
                    float y = (e.Y - rect.Y) * 1.0F / rect.Height;
                    if (y > 0) Value = (int)Math.Round(y * _maxValue);
                    else Value = 0;
                }
                else
                {
                    float x = (e.X - rect.X) * 1.0F / rect.Width;
                    if (x > 0) Value = (int)Math.Round(x * _maxValue);
                    else Value = 0;
                }
            }
        }

        bool mouseFlat = false;
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            mouseFlat = false;
            Invalidate();
        }

        float AnimationHoverValue = 0F;
        bool AnimationHover = false;
        bool _mouseHover = false;
        bool ExtraMouseHover
        {
            get => _mouseHover;
            set
            {
                if (_mouseHover == value) return;
                _mouseHover = value;
                var enabled = Enabled;
                SetCursor(value && enabled);
                if (Config.Animation)
                {
                    ThreadHover?.Dispose();
                    AnimationHover = true;
                    if (value)
                    {
                        ThreadHover = new ITask(this, () =>
                        {
                            AnimationHoverValue = AnimationHoverValue.Calculate(0.1F);
                            if (AnimationHoverValue > 1) { AnimationHoverValue = 1; return false; }
                            Invalidate();
                            return true;
                        }, 10, () =>
                        {
                            AnimationHover = false;
                            Invalidate();
                        });
                    }
                    else
                    {
                        ThreadHover = new ITask(this, () =>
                        {
                            AnimationHoverValue = AnimationHoverValue.Calculate(-0.1F);
                            if (AnimationHoverValue <= 0) { AnimationHoverValue = 0F; return false; }
                            Invalidate();
                            return true;
                        }, 10, () =>
                        {
                            AnimationHover = false;
                            Invalidate();
                        });
                    }
                }
                Invalidate();
            }
        }

        #region 动画

        protected override void Dispose(bool disposing)
        {
            ThreadHover?.Dispose();
            base.Dispose(disposing);
        }
        ITask? ThreadHover = null;

        #endregion

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            ExtraMouseHover = true;
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            CloseTips();
            ExtraMouseHover = false;
        }
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            CloseTips();
            ExtraMouseHover = false;
        }

        #endregion
    }
}