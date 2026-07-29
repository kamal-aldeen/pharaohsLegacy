namespace pharaohsLegacy.Models
{
    // ============================================================================
    // 🆕 بند 17 — أيقونات SVG مخصّصة للشارات (بدل Font Awesome)
    // كل أيقونة Line-Art بسيطة بلون واحد (currentColor) عشان تورّث لون الذهبي من
    // الـ CSS المحيطة بيها تلقائيًا (Earned = ذهبي كامل، Locked = رمادي باهت زي باقي
    // الكارت — من غير ما نكتب لون تاني لكل حالة). بتتنادى بمفتاح الشارة (Badge.Key).
    // ============================================================================
    public static class BadgeIconSvg
    {
        private const string Attrs = "viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.6\" stroke-linecap=\"round\" stroke-linejoin=\"round\"";

        private static readonly Dictionary<string, string> Icons = new()
        {
            // Visit
            ["explorer"] = $"<svg {Attrs}><circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M15 9l-2.2 5.2L9 16l2.2-5.2z\"/></svg>",

            // Knowledge
            ["pharaoh_expert"] = $"<svg {Attrs}><path d=\"M4 18l1.6-8.4L9 14l3-7 3 7 3.4-4.4L20 18z\"/><path d=\"M4 18h16\"/></svg>",
            ["dynasty_expert"] = $"<svg {Attrs}><rect x=\"5\" y=\"4\" width=\"14\" height=\"16\" rx=\"2\"/><path d=\"M9 9h6M9 13h6M9 17h3\"/></svg>",
            ["quiz_master"] = $"<svg {Attrs}><path d=\"M9 18h6\"/><path d=\"M10 21h4\"/><path d=\"M12 3a6 6 0 00-3.2 11.1c.6.5 1.2 1.3 1.2 2.4v.5h4v-.5c0-1.1.6-1.9 1.2-2.4A6 6 0 0012 3z\"/></svg>",

            // Community
            ["reviewer"] = $"<svg {Attrs}><path d=\"M4 20l4.5-1 10-10a2.1 2.1 0 00-3-3l-10 10z\"/><path d=\"M14.5 6.5l3 3\"/></svg>",
            ["community_helper"] = $"<svg {Attrs}><circle cx=\"8.5\" cy=\"8\" r=\"2.8\"/><circle cx=\"16\" cy=\"8.5\" r=\"2.3\"/><path d=\"M3.5 19c0-3.3 2.3-5.5 5-5.5s5 2.2 5 5.5\"/><path d=\"M13 19c.2-2.6 1.9-4.3 4-4.3s3.8 1.7 4 4.3\"/></svg>",

            // Legendary
            ["legendary_explorer"] = $"<svg {Attrs}><path d=\"M7 4h10v4a5 5 0 01-10 0V4z\"/><path d=\"M5 5H3.5v1.5A3.5 3.5 0 007 10M19 5h1.5v1.5A3.5 3.5 0 0117 10\"/><path d=\"M12 13v4\"/><path d=\"M9 21h6\"/><path d=\"M10 17h4v1.5a2 2 0 01-2 2 2 2 0 01-2-2z\"/></svg>",

            // Hidden
            ["perfect_score"] = $"<svg {Attrs}><path d=\"M12 3l2.5 5.5L20 9.3l-4 4 1 5.7L12 16.5 7 19l1-5.7-4-4 5.5-.8z\"/></svg>",
            ["streak_legend"] = $"<svg {Attrs}><path d=\"M12 3c1.2 2.6-1.2 3.8-1.2 6.4a3.2 3.2 0 006.4 0c0-1.6-.8-2.4-.8-2.4s1.6.9 1.6 4A6 6 0 016 11c0-4.2 3.2-5.6 3.2-7.3 0 0 1.6.8 2.8-.7z\"/></svg>",
            ["museum_completionist"] = $"<svg {Attrs}><path d=\"M3 9l9-5 9 5\"/><path d=\"M4.5 9v11M8.5 9v11M12 9v11M15.5 9v11M19.5 9v11\"/><path d=\"M3 20h18\"/></svg>",
            ["true_historian"] = $"<svg {Attrs}><circle cx=\"12\" cy=\"6.5\" r=\"3\"/><path d=\"M12 9.5V21M7.5 14h9\"/></svg>",
            ["loyal_explorer"] = $"<svg {Attrs}><path d=\"M12 20.5s-7-4.3-9.2-8.7C1.3 8.6 2.8 5 6.4 5c2 0 3.3 1.3 3.9 2.3.6-1 1.9-2.3 3.9-2.3 3.6 0 5.1 3.6 3.6 6.8-2.2 4.4-9.2 8.7-9.2 8.7z\"/></svg>",
            ["night_owl"] = $"<svg {Attrs}><path d=\"M20 14.2A8.3 8.3 0 1110 4a6.8 6.8 0 0010 10.2z\"/></svg>",
        };

        private static readonly string Fallback = $"<svg {Attrs}><circle cx=\"12\" cy=\"12\" r=\"8\"/></svg>";

        public static string Get(string badgeKey)
        {
            return Icons.TryGetValue(badgeKey, out var svg) ? svg : Fallback;
        }
    }
}
