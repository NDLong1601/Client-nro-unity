using System;
using System.Collections.Generic;
using Game2.UI.Adapters;
using Game2.UI.Components;
using Nro.UI;

namespace Game2.UI.CustomMenu
{
    public class CustomMenuScr : mScreen
    {
        private static CustomMenuScr _instance;
        private static bool _isOpen;

        private mScreen _previousScreen;
        private UiInputContext _inputContext;

        public static bool IsOpen
        {
            get { return _isOpen; }
        }

        // Tabs state
        private int _selectedMainTab = 0; // 0: Nhiệm vụ, 1: Hành trang, 2: Kỹ năng, 3: Bang hội, 4: Chức năng
        private int _selectedSubTab = 0;  // 0: Nhiệm vụ chính, 1: Nhiệm vụ khác
        private int _selectedTaskPosition = 0;
        private int _selectedOtherCategoryIndex = 0; // 0: Bò Mộng, 1: Kanao, 2: Ngư Dân, 3: Bang Hội
        private int _keyboardFocus = KeyboardFocusContent;

        // Scroll adapters
        private ScrollViewAdapter _leftScrollAdapter;
        private ScrollViewAdapter _rightScrollAdapter;

        // Layout bounds
        private UiRect _frameRect;
        private UiRect _tabBarRect;
        private UiRect _footerRect;
        private UiRect _contentRect;

        // Task tab layout bounds
        private UiRect _subTab0Rect;
        private UiRect _subTab1Rect;
        private UiRect _leftColRect;
        private UiRect _rightColRect;
        private UiRect _rightBodyRect;
        private UiRect _closeBtnRect;
        private readonly UiRect[] _otherQuestCardRects = new UiRect[4];

        private const int MainTabCount = 5;
        private const int MaxFrameWidth = 460;
        private const int MaxFrameHeight = 242;
        private const int SidebarWidth = 70;
        private const int FooterHeight = 27;
        private const int MainTaskRowHeight = 33;
        private const int OtherTaskRowHeight = 34;
        private const int LastKnownMainTaskId = 29;
        private const int KeyboardFocusMainTabs = 0;
        private const int KeyboardFocusContent = 1;

        private static Image[] _mainTabIcons;
        private static Image[] _otherTaskIcons;
        private static Image[] _statusIcons;

        private static readonly string[] MainTabNames = new string[]
        {
            "Nhiệm vụ",
            "Hành trang",
            "Kỹ năng",
            "Bang hội",
            "Chức năng"
        };

        // Matches the visual order in PaintOtherQuestGroup: daily, infinity, clan, fishing.
        private static readonly int[] OtherQuestNavigationOrder = new int[] { 0, 1, 3, 2 };

        // Canonical Main Task Names in Dragon Boy / Teamobi NRO (task_main_template)
        public static readonly string[] MainTaskNames = new string[]
        {
            "Nhiệm vụ đầu tiên",                 // 0
            "Nhiệm vụ tập luyện",                // 1
            "Nhiệm vụ tìm thức ăn",              // 2
            "Nhiệm vụ sao băng",                 // 3
            "Nhiệm vụ thử thách",                // 4
            "Nhiệm vụ thử thách",                // 5
            "Nhiệm vụ thử thách",                // 6
            "Nhiệm vụ giải cứu",                 // 7
            "Nhiệm vụ tìm ngọc",                 // 8
            "Nhiệm vụ bái sư",                   // 9
            "Nhiệm vụ thử sức",                  // 10
            "Nhiệm vụ gia tăng sức mạnh",        // 11
            "Nhiệm vụ xin phép",                 // 12
            "Nhiệm vụ gia nhập bang hội",        // 13
            "Nhiệm vụ bang hội đầu tiên",        // 14
            "Nhiệm vụ bang hội thứ 2",           // 15
            "Tiêu diệt quái vật",                // 16
            "Nhiệm vụ giúp đỡ Cui",              // 17
            "Nhiệm vụ bất khả thi",              // 18
            "Nhiệm vụ tìm diệt đệ tử",           // 19
            "Nhiệm vụ Tiểu đội sát thủ",         // 20
            "Nhiệm vụ chạm trán Fide đại ca",    // 21
            "Chú bé đến từ tương lai",           // 22
            "Chạm chán Robot sát thủ lần 1",     // 23
            "Chạm trán Robot sát thủ lần 2",     // 24
            "Chạm trán Robot sát thủ lần 3",     // 25
            "Chạm trán Xên bọ hung",             // 26
            "Cuộc dạo chơi của xên",             // 27
            "Cuộc đối đầu không cân sức",        // 28
            "Trở về quá khứ",                    // 29
            "Đạt 1 tỷ sức mạnh",                 // 30
            "Thử thách Thần Hủy Diệt",           // 31
            "Thu thập ngọc rồng siêu cấp"        // 32
        };

        // Guidance is kept for every task so completed rows remain as informative as the active row.
        public static readonly string[] CanonicalTaskGuides = new string[]
        {
            "Đi theo mũi tên chỉ dẫn, gặp người thân tại nhà, mở rương đồ, thu hoạch đậu thần rồi quay lại báo cáo.",
            "Mộc nhân ở ngay trước nhà tại làng. Đánh ngã 5 mộc nhân rồi về báo cáo; chạm nhanh hai lần vào mục tiêu để tấn công.",
            "Đến đồi hoang của hành tinh, tiêu diệt quái và nhặt đủ 10 đùi gà rồi quay về báo cáo.",
            "Học kỹ năng bay bằng tiềm năng, sau đó đi kiểm tra vật thể lạ vừa rơi xuống hành tinh.",
            "Khủng long mẹ ở Trái Đất, lợn lòi mẹ ở Namếc và quỷ đất mẹ ở Xayda. Dùng tàu vũ trụ để đổi hành tinh.",
            "Lợn lòi mẹ ở Namếc, khủng long mẹ ở Trái Đất và quỷ đất mẹ ở Xayda. Dùng tàu vũ trụ để đổi hành tinh.",
            "Quỷ đất mẹ ở Xayda, khủng long mẹ ở Trái Đất và lợn lòi mẹ ở Namếc. Dùng tàu vũ trụ để đổi hành tinh.",
            "Đến khu vực được chỉ dẫn, hạ 20 quái bay rồi quay về làng báo cáo.",
            "Ngọc rồng 7 sao đã bị quái vật cướp. Đánh bại chúng, tìm lại ngọc và mang về giao cho người thân.",
            "Tìm đường đến Đảo Kamê, Đảo Guru hoặc Vách núi đen; nói chuyện với sư phụ của hành tinh để xin làm đệ tử.",
            "Luyện đủ sức mạnh, tiêu diệt số quái được yêu cầu để thể hiện thực lực rồi báo cáo với sư phụ.",
            "Ra ngoài luyện tập và đạt lần lượt các mốc sức mạnh được giao, sau đó trở về nhà.",
            "Quay về nhà gặp ông Gôhan, Moori hoặc Paragus để xin phép tham gia bang hội.",
            "Tạo hoặc gia nhập một bang hội có đồng đội thiện chí, cùng nhau làm nhiệm vụ rồi báo cáo với sư phụ.",
            "Phối hợp với một đồng đội để tiến trình nhanh gấp đôi. Heo rừng ở Rừng Bamboo, heo da xanh ở Núi hoa vàng, heo Xayda ở Rừng cọ; dùng tàu vũ trụ để di chuyển.",
            "Phối hợp với một đồng đội để tiến trình nhanh gấp đôi; tiêu diệt Bulon, Ukulele và Quỷ mập rồi quay về báo cáo.",
            "Tới các hành tinh, lần lượt tiêu diệt Tambourine, Drum và Akkuman để giải cứu thường dân rồi báo cáo với sư phụ.",
            "Tìm đường tới Thành phố Vegeta trên hành tinh Xayda, gặp và nói chuyện với Cui.",
            "Đạt 50 triệu sức mạnh rồi tới Xayda tiêu diệt Nappa, Soldier, Appule, Raspberry và Thằn lằn xanh.",
            "Tiêu diệt Kuku, Mập Đầu Đinh và Rambo tại Xayda. Nếu chưa tìm thấy, hãy gặp Cui ở Thành phố Vegeta để hỏi vị trí.",
            "Tới Xayda tiêu diệt các thành viên Tiểu Đội Sát Thủ do Fide gọi đến, sau đó báo cáo với sư phụ.",
            "Luyện tập đạt 2 tỷ sức mạnh rồi lên đường tìm và đánh bại các hình thái của Fide đại ca.",
            "Đến Trái Đất, tìm người lạ tại Rừng Bamboo, Rừng dương xỉ và Nam Kamê; đưa thuốc cho Quy Lão rồi theo Ca Lích tới tương lai diệt bọ hung con.",
            "Tìm Rôbốt Sát Thủ tại Thành phố phía Nam, Đảo Balê hoặc Cao nguyên; cùng hai đồng bang diệt Xên con cấp 3 rồi báo Bunma tương lai.",
            "Trở về quá khứ, tới sân sau siêu thị, tiêu diệt Android 15, Android 14 và Android 13 rồi báo Bunma tương lai.",
            "Đến Thành phố phía Bắc, Ngọn núi phía Bắc và Thung lũng phía Bắc; diệt Rôbốt Sát Thủ và Xên con cấp 5 rồi báo Bunma tương lai.",
            "Đến thị trấn Ginder, tiêu diệt các hình thái Xên Bọ Hung và Xên con cấp 8 rồi báo Bunma tương lai.",
            "Nâng sức đánh gốc, thu thập Capsule kì bí, tới võ đài Xên Bọ Hung và cẩn thận với những vị khách nguy hiểm.",
            "Vào 12 giờ trưa hằng ngày, đến map Đại hội võ thuật gặp Ô Sin và làm theo chỉ dẫn.",
            "Gặp Ca Lích tại tương lai, dùng cỗ máy thời gian trở về quá khứ rồi báo cáo với Quy Lão Kame.",
            "Luyện tập không ngừng để đạt 1 tỷ sức mạnh rồi quay về báo cáo với sư phụ.",
            "Đến Thánh địa Beerus, vượt thử thách của Whis và giao đấu với Thần Hủy Diệt Beerus.",
            "Tìm đủ 7 viên ngọc rồng siêu cấp trong vũ trụ để triệu hồi Rồng Thần Zarama."
        };

        // Canonical Sub-Steps for all Main Tasks
        public static readonly string[][] CanonicalTaskSubSteps = new string[][]
        {
            new string[] { "Di chuyển tới mũi tên chỉ dẫn", "Hãy đi đến nhà ông Gôhan / Moori / Paragus", "Nói chuyện với ông Gôhan / Moori / Paragus", "Mở rương đồ", "Thu hoạch đậu thần", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Đánh ngã 5 mộc nhân", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Thu thập 10 đùi gà tại đồi hoang", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Sử dụng tiềm năng học kỹ năng bay", "Đi khám phá vật thể lạ rơi xuống", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Đánh 3 con khủng long mẹ (Trái Đất)", "Đánh 3 con lợn lòi mẹ (Namếc)", "Đánh 3 con quỷ đất mẹ (Xayda)", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Đánh 3 con lợn lòi mẹ (Namếc)", "Đánh 3 con khủng long mẹ (Trái Đất)", "Đánh 3 con quỷ đất mẹ (Xayda)", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Đánh 3 con quỷ đất mẹ (Xayda)", "Đánh 3 con khủng long mẹ (Trái Đất)", "Đánh 3 con lợn lòi mẹ (Namếc)", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Đạt 16.000 sức mạnh", "Đánh bại 20 con quái bay (Phi long / Quỷ bay / Thằn lằn bay)", "Nói chuyện với NPC tại làng", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Đạt 40.000 sức mạnh", "Tìm kiếm viên ngọc rồng 7 sao", "Đem ngọc rồng về cho ông Gôhan / Moori / Paragus" },
            new string[] { "Lên đường tới Đảo Kamê / Đảo Guru / Vách núi đen", "Chào hỏi và bái Quy Lão Kame / Guru / Vua Vegeta làm sư phụ" },
            new string[] { "Đạt 200.000 sức mạnh", "Tiêu diệt 10 con quái thể hiện sức mạnh", "Báo cáo kết quả với sư phụ" },
            new string[] { "Đạt 500.000 sức mạnh", "Đạt 550.000 sức mạnh", "Đạt 600.000 sức mạnh", "Đi về nhà thăm ông Gôhan / Moori / Paragus" },
            new string[] { "Đi về nhà gặp ông Gôhan / Moori / Paragus", "Nói chuyện xin phép gia nhập bang hội", "Báo cáo lại cho sư phụ" },
            new string[] { "Tạo hoặc gia nhập 1 bang hội có từ 2 thành viên", "Báo cáo hoàn tất cho sư phụ" },
            new string[] { "Tiêu diệt 30 con heo rừng tại Rừng Bamboo", "Tiêu diệt 30 con heo da xanh tại Núi hoa vàng", "Tiêu diệt 30 con heo xayda tại Rừng cọ", "Quay về báo cáo nhiệm vụ cho sư phụ" },
            new string[] { "Tiêu diệt 30 Bulon", "Tiêu diệt 30 Ukulele", "Tiêu diệt 30 Quỷ mập", "Quay về báo cáo nhiệm vụ cho sư phụ" },
            new string[] { "Tiêu diệt Tambourine", "Tiêu diệt Drum", "Tiêu diệt Akkuman", "Quay về báo cáo nhiệm vụ cho sư phụ" },
            new string[] { "Tìm đường tới Thành Phố Vegeta", "Gặp gỡ và nói chuyện với Cui" },
            new string[] { "Tiêu diệt 500 Nappa", "Tiêu diệt 400 Soldier", "Tiêu diệt 300 Appule", "Tiêu diệt 200 Raspberry", "Tiêu diệt 100 Thằn lằn xanh", "Báo cáo kết quả với sư phụ" },
            new string[] { "Tiêu diệt Kuku", "Tiêu diệt Mập Đầu Đinh", "Tiêu diệt Rambo", "Trả nhiệm vụ cho sư phụ" },
            new string[] { "Đạt 600 triệu sức mạnh", "Tiêu diệt Số 4 (Guldo)", "Tiêu diệt Số 3 (Recoome)", "Tiêu diệt Số 2 (Burter)", "Tiêu diệt Số 1 (Jeice)", "Tiêu diệt Tiểu Đội Trưởng (Ginyu)", "Báo cáo với sư phụ" },
            new string[] { "Đạt 2 tỷ sức mạnh", "Tiêu diệt Fide cấp 1", "Tiêu diệt Fide cấp 2", "Tiêu diệt Fide cấp 3", "Báo cáo với sư phụ" },
            new string[] { "Báo cáo với ông Gôhan / Moori / Paragus", "Đi tìm vị khách lạ (Ca Lích)", "Đưa thuốc trợ tim cho Quy Lão", "Đến tương lai gặp Bunma", "Diệt 1000 xên con cấp 1", "Báo với Bunma tương lai" },
            new string[] { "Đến điểm hẹn tìm Rôbốt Sát Thủ", "Tiêu diệt Số 2 (Android 19)", "Tiêu diệt Số 1 (Android 20)", "Diệt 900 xên con cấp 3", "Báo với Bunma tương lai" },
            new string[] { "Đến sân sau siêu thị", "Tiêu diệt Android 15", "Tiêu diệt Android 14", "Tiêu diệt Android 13", "Báo với Bunma tương lai" },
            new string[] { "Đi tìm Píc Póc", "Tiêu diệt Póc", "Tiêu diệt Píc", "Tiêu diệt Kinh Kong", "Diệt 800 xên con cấp 5", "Báo với Bunma tương lai" },
            new string[] { "Đến thị trấn Ginder", "Tiêu diệt Xên Bọ Hung cấp 1", "Tiêu diệt Xên Bọ Hung cấp 2", "Tiêu diệt Xên Bọ Hung hoàn thiện", "Diệt 700 xên con cấp 8", "Báo với Bunma tương lai" },
            new string[] { "Nâng sức đánh gốc lên 10.000", "Thu thập 50 Capsule kì bí", "Đến võ đài Xên Bọ Hung", "Tiêu diệt 7 đứa con của Xên", "Tiêu diệt Siêu Bọ Hung", "Báo với Bunma tương lai" },
            new string[] { "Gặp và đi theo Ôsin tại Đại Hội Võ Thuật", "Hạ vua địa ngục Drabura (0/10)", "Hạ Pui Pui (0/10)", "Hạ Pui Pui lần 2 (0/10)", "Hạ Yacôn (0/10)", "Hạ Drabura lần 2 (0/10)", "Hạ Ma Bư (0/10)", "Báo cáo kết quả với Ôsin" },
            new string[] { "Gặp Ca Lích tại tương lai", "Khởi hành cỗ máy thời gian trở về quá khứ", "Báo cáo với Quy Lão Kame" },
            new string[] { "Luyện tập không ngừng đạt mốc 1 tỷ sức mạnh", "Báo cáo với sư phụ" },
            new string[] { "Đến Thánh địa Beerus", "Vượt qua thử thách của Whis", "Giao đấu cùng Thần Hủy Diệt Beerus" },
            new string[] { "Tìm kiếm 7 viên ngọc rồng siêu cấp vũ trụ", "Triệu hồi Rồng Thần Siêu Cấp Zarama", "Ước 1 điều ước vĩ đại" }
        };

        // Canonical Rewards for all Main Tasks
        public static readonly string[][] CanonicalTaskRewards = new string[][]
        {
            new string[] { "500 sức mạnh", "500 tiềm năng" },
            new string[] { "1.000 sức mạnh", "1.000 tiềm năng", "200 triệu vàng" },
            new string[] { "1.500 sức mạnh", "1.500 tiềm năng", "300 triệu vàng", "Học kỹ năng bay" },
            new string[] { "2.000 sức mạnh", "2.000 tiềm năng", "400 triệu vàng" },
            new string[] { "4.000 sức mạnh", "4.000 tiềm năng" },
            new string[] { "4.000 sức mạnh", "4.000 tiềm năng" },
            new string[] { "4.000 sức mạnh", "4.000 tiềm năng" },
            new string[] { "8.000 sức mạnh", "8.000 tiềm năng" },
            new string[] { "15.000 sức mạnh", "15.000 tiềm năng" },
            new string[] { "20.000 tiềm năng", "Bái sư thành công" },
            new string[] { "30.000 tiềm năng", "500.000 vàng" },
            new string[] { "50.000 tiềm năng", "1.000.000 vàng" },
            new string[] { "80.000 tiềm năng", "2.000.000 vàng" },
            new string[] { "100.000 tiềm năng", "Capsule 1 chỗ" },
            new string[] { "150.000 tiềm năng", "Đùi gà nướng" },
            new string[] { "200.000 tiềm năng", "Rương đồ thần bí" },
            new string[] { "300.000 tiềm năng", "10 ngọc xanh" },
            new string[] { "500.000 tiềm năng", "Capsule đặc biệt" },
            new string[] { "1.000.000 tiềm năng", "50 ngọc xanh" },
            new string[] { "2.000.000 tiềm năng", "Đậu thần cấp 7" },
            new string[] { "5.000.000 tiềm năng", "Găng tay vàng" },
            new string[] { "10.000.000 tiềm năng", "100 ngọc xanh" },
            new string[] { "20.000.000 tiềm năng", "Bộ ngọc rồng 3 sao" },
            new string[] { "30.000.000 tiềm năng", "Capsule Vàng" },
            new string[] { "40.000.000 tiềm năng", "Radar dò ngọc" },
            new string[] { "50.000.000 tiềm năng", "200 ngọc xanh" },
            new string[] { "60.000.000 tiềm năng", "Cải trang Xên bọ hung" },
            new string[] { "80.000.000 tiềm năng", "Capsule Kỳ Bí" },
            new string[] { "100.000.000 tiềm năng", "Kiếm Z huyền thoại" },
            new string[] { "150.000.000 tiềm năng", "Bộ ngọc rồng Namếc" },
            new string[] { "200.000.000 tiềm năng", "Danh hiệu Chiến Thần" },
            new string[] { "300.000.000 tiềm năng", "Đồ Thần Linh" },
            new string[] { "500.000.00 điều ước Rồng Siêu Cấp", "1.000 ngọc xanh" }
        };

        // Multi-NPC Quest Model
        public class NpcQuest
        {
            public string key;
            public string categoryName;
            public string npcName;
            public bool hasQuest;
            public string title;
            public string goal;
            public int count;
            public int maxCount;
            public string defaultHint;
            public string summary;
            public string[] rewards;
            public bool isComplete;
            public int iconType; // 0: BoMong, 1: Kanao, 2: Fishing, 3: Clan
        }

        public static NpcQuest questBoMong = new NpcQuest
        {
            key = "bomong",
            categoryName = "Nhiệm vụ hằng ngày",
            npcName = "Bò Mộng",
            hasQuest = false,
            title = "Nhiệm vụ hằng ngày",
            goal = "Chưa nhận nhiệm vụ hằng ngày từ Bò Mộng.",
            iconType = 0,
            rewards = new string[] { "10.000 Ngọc xanh", "50.000.000 Vàng", "Điểm năng động hằng ngày", "Huy hiệu Nông Dân Chăm Chỉ (Cấp Địa Ngục)" },
            summary = "Hoàn thành nhiệm vụ mỗi ngày để nhận thêm phần thưởng.",
            defaultHint = "Đến gặp NPC Bò Mộng tại Làng Aru / Làng Mori / Làng Kakarot để chọn độ khó (Dễ, Bình thường, Khó) và nhận nhiệm vụ. Sau khi hoàn thành mục tiêu, quay lại gặp Bò Mộng để trả nhiệm vụ và nhận thưởng."
        };

        public static NpcQuest questKanao = new NpcQuest
        {
            key = "kanao",
            categoryName = "Nhiệm vụ hằng ngày",
            npcName = "Kanao",
            hasQuest = false,
            title = "Nhiệm vụ Vô Hạn Thành",
            goal = "Chưa nhận nhiệm vụ Vô Hạn Thành từ Kanao.",
            iconType = 1,
            rewards = new string[] { "Huyết Quỷ", "Rương Vô Hạn Thành", "50.000.000 Tiềm năng", "Điểm cống hiến sự kiện" },
            summary = "Tiêu diệt mục tiêu trong Vô Hạn Thành rồi quay lại Kanao.",
            defaultHint = "Đến gặp Kanao tại Làng Aru hoặc map 187 (Vô Hạn Thành) để nhận nhiệm vụ diệt quái ngẫu nhiên. Di chuyển vào map tương ứng trong Vô Hạn Thành để săn quái vật. Khi hoàn thành, gặp lại Kanao và chọn Nhận thưởng."
        };

        public static NpcQuest questFishing = new NpcQuest
        {
            key = "fishing",
            categoryName = "Nhiệm vụ câu cá",
            npcName = "Ngư Dân",
            hasQuest = false,
            title = "Nhiệm vụ câu cá",
            goal = "Chưa nhận nhiệm vụ câu cá.",
            iconType = 2,
            rewards = new string[] { "60 Xu Ngư Phủ", "5 Mồi Tôm Biển", "1 Huy Hiệu Ngư Phủ", "Điểm kinh nghiệm câu cá" },
            summary = "Câu đủ số cá yêu cầu rồi trở về gặp Ngư Dân.",
            defaultHint = "Đến Làng Chài hoặc Đảo Kamê gặp Ngư Dân để nhận nhiệm vụ câu cá theo độ khó (Dễ, Vừa, Khó, Cực khó). Chuẩn bị cần câu và mồi câu thích hợp tại các ngư trường. Khi câu cá hoàn thành, quay lại gặp Ngư Dân để nhận thưởng."
        };

        public static NpcQuest questClan = new NpcQuest
        {
            key = "clan",
            categoryName = "Nhiệm vụ bang hội",
            npcName = "Sư Phụ",
            hasQuest = false,
            title = "Nhiệm vụ bang hội",
            goal = "Chưa nhận nhiệm vụ bang hội.",
            iconType = 3,
            rewards = new string[] { "Capsule Bang hội", "Tiềm năng sức mạnh", "Điểm cống hiến bang hội", "Quỹ phúc lợi bang" },
            summary = "Cùng thành viên hoàn thành mục tiêu của bang hội.",
            defaultHint = "Yêu cầu đã gia nhập một Bang hội có từ 2 thành viên trở lên. Đến gặp Sư phụ của hành tinh (Quy Lão Kame tại Đảo Kamê, Trưởng lão Guru tại Đảo Guru, hoặc Vua Vegeta tại Hành tinh Vegeta) để nhận nhiệm vụ bang. Khi hoàn thành, quay về báo cáo với Bang hội."
        };

        public static readonly NpcQuest[] allOtherQuests = new NpcQuest[]
        {
            questBoMong,
            questKanao,
            questFishing,
            questClan
        };

        public static void Open()
        {
            if (_instance == null)
            {
                _instance = new CustomMenuScr();
            }
            if (_isOpen) return;
            _instance._previousScreen = GameCanvas.currentScreen;
            _instance.Init();
            _isOpen = true;
            GameCanvas.clearKeyPressed();
            GameCanvas.clearKeyHold();
        }

        public static void Toggle()
        {
            if (_isOpen && _instance != null)
            {
                _instance.Close();
                return;
            }
            Open();
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;
            _inputContext?.ConsumePointer();
            _inputContext?.Reset();
            _previousScreen = null;
            GameCanvas.clearKeyPressed();
            GameCanvas.clearKeyHold();
        }

        public static void UpdateOverlay()
        {
            if (!_isOpen || _instance == null) return;
            if (_instance._previousScreen != null && !object.ReferenceEquals(GameCanvas.currentScreen, _instance._previousScreen))
            {
                _instance.Close();
                return;
            }
            _instance.update();
            _instance.updateKey();
        }

        public static void PaintOverlay(mGraphics g)
        {
            if (!_isOpen || _instance == null) return;
            if (_instance._previousScreen != null && !object.ReferenceEquals(GameCanvas.currentScreen, _instance._previousScreen)) return;
            _instance.paint(g);
        }

        private bool HandleKeyboardNavigation()
        {
            if (GameCanvas.keyPressed[23] || GameCanvas.keyPressed[4])
            {
                ConsumeDirectionKeys(23, 4);
                MoveHorizontalFocus(-1);
                return true;
            }
            if (GameCanvas.keyPressed[24] || GameCanvas.keyPressed[6])
            {
                ConsumeDirectionKeys(24, 6);
                MoveHorizontalFocus(1);
                return true;
            }
            if (GameCanvas.keyPressed[21] || GameCanvas.keyPressed[2])
            {
                ConsumeDirectionKeys(21, 2);
                MoveVerticalSelection(-1);
                return true;
            }
            if (GameCanvas.keyPressed[22] || GameCanvas.keyPressed[8])
            {
                ConsumeDirectionKeys(22, 8);
                MoveVerticalSelection(1);
                return true;
            }
            return false;
        }

        private static void ConsumeDirectionKeys(int arrowKey, int legacyKey)
        {
            GameCanvas.keyPressed[arrowKey] = false;
            GameCanvas.keyPressed[legacyKey] = false;
            GameCanvas.keyHold[arrowKey] = false;
            GameCanvas.keyHold[legacyKey] = false;
        }

        private void MoveHorizontalFocus(int direction)
        {
            if (_keyboardFocus == KeyboardFocusMainTabs)
            {
                if (direction <= 0) return;
                _keyboardFocus = KeyboardFocusContent;
                if (_selectedMainTab == 0)
                {
                    _selectedSubTab = 0;
                    RefreshKeyboardPage();
                }
                SoundMn.gI().panelClick();
                return;
            }

            if (_selectedMainTab != 0)
            {
                if (direction < 0)
                {
                    _keyboardFocus = KeyboardFocusMainTabs;
                    SoundMn.gI().panelClick();
                }
                return;
            }

            if (direction > 0)
            {
                if (_selectedSubTab < 1)
                {
                    _selectedSubTab++;
                    if (!allOtherQuests[_selectedOtherCategoryIndex].hasQuest) SelectFirstAvailableOtherQuest();
                    RefreshKeyboardPage();
                    SoundMn.gI().panelClick();
                }
                return;
            }

            if (_selectedSubTab > 0)
            {
                _selectedSubTab--;
                RefreshKeyboardPage();
            }
            else
            {
                _keyboardFocus = KeyboardFocusMainTabs;
            }
            SoundMn.gI().panelClick();
        }

        private void MoveVerticalSelection(int direction)
        {
            if (_keyboardFocus == KeyboardFocusMainTabs)
            {
                _selectedMainTab = (_selectedMainTab + direction + MainTabCount) % MainTabCount;
                if (_selectedMainTab == 0) _selectedSubTab = 0;
                RefreshKeyboardPage();
                SoundMn.gI().panelClick();
                return;
            }

            MoveRowSelection(direction);
        }

        private void RefreshKeyboardPage()
        {
            _leftScrollAdapter?.Reset();
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();
            if (_selectedMainTab == 0 && _selectedSubTab == 0)
                _leftScrollAdapter?.ScrollToIndex(_selectedTaskPosition);
        }

        private void MoveRowSelection(int direction)
        {
            if (_selectedMainTab != 0) return;

            if (_selectedSubTab == 0)
            {
                int rowCount = GetMainTaskRowCount();
                if (rowCount <= 0) return;
                _selectedTaskPosition = (_selectedTaskPosition + direction + rowCount) % rowCount;
                _rightScrollAdapter?.Reset();
                ConfigureScrollAdapters();
                _leftScrollAdapter?.ScrollToIndex(_selectedTaskPosition);
                SoundMn.gI().panelClick();
                return;
            }

            List<int> visibleQuests = new List<int>();
            for (int i = 0; i < OtherQuestNavigationOrder.Length; i++)
            {
                int questIndex = OtherQuestNavigationOrder[i];
                if (allOtherQuests[questIndex].hasQuest) visibleQuests.Add(questIndex);
            }
            if (visibleQuests.Count == 0) return;

            int currentPosition = visibleQuests.IndexOf(_selectedOtherCategoryIndex);
            int nextPosition = currentPosition < 0
                ? (direction > 0 ? 0 : visibleQuests.Count - 1)
                : (currentPosition + direction + visibleQuests.Count) % visibleQuests.Count;
            _selectedOtherCategoryIndex = visibleQuests[nextPosition];
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();

            UiRect selectedRect = _otherQuestCardRects[_selectedOtherCategoryIndex];
            if (selectedRect.Width > 0 && _leftScrollAdapter != null)
            {
                int contentOffset = selectedRect.Y - _leftColRect.Y + _leftScrollAdapter.ScrollY;
                _leftScrollAdapter.ScrollToIndex(contentOffset / 10);
            }
            SoundMn.gI().panelClick();
        }

        // ----------------------------------------------------
        // Real-Time Server Event Listeners (Hooked in Controller)
        // ----------------------------------------------------
        public static void SyncMainTask()
        {
            if (_instance == null) return;
            int currentTaskId = GetCurrentTaskId();
            _instance._selectedTaskPosition = GetMainTaskPosition(currentTaskId);
            _instance.ConfigureScrollAdapters();
            _instance._leftScrollAdapter?.ScrollToIndex(_instance._selectedTaskPosition);
        }

        public static void OnNpcDialog(int npcTempId, string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            // 1. Ngư Dân (Fishing)
            if (npcTempId == 113 || text.Contains("cá cấp") || text.Contains("Cá cấp") || text.Contains("Ngư dân") || text.Contains("Ngư Phủ"))
            {
                if (text.Contains("Mỗi ngày được nhận tối đa") && !text.Contains("Tiến độ:"))
                {
                    questFishing.hasQuest = false;
                    questFishing.isComplete = false;
                    questFishing.count = 0;
                    questFishing.maxCount = 0;
                    questFishing.title = "Nhiệm vụ câu cá";
                    questFishing.goal = "Chưa nhận nhiệm vụ câu cá.";
                }
                else if (text.Contains("Nhiệm vụ") && text.Contains("Tiến độ:"))
                {
                    questFishing.hasQuest = true;
                    string[] lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string l = lines[i].Trim();
                        if (l.StartsWith("Nhiệm vụ"))
                        {
                            questFishing.title = l;
                        }
                        else if (l.Contains("cá cấp") || l.Contains("Cá cấp") || l.StartsWith("Câu"))
                        {
                            questFishing.goal = l;
                        }
                        else if (l.Contains("Tiến độ:"))
                        {
                            string p = l.Replace("Tiến độ:", "").Trim();
                            string[] parts = p.Split('/');
                            if (parts.Length == 2)
                            {
                                int.TryParse(parts[0].Trim(), out questFishing.count);
                                int.TryParse(parts[1].Trim(), out questFishing.maxCount);
                            }
                        }
                        else if (l.Contains("hoàn thành") || l.Contains("nhận thưởng"))
                        {
                            questFishing.isComplete = true;
                        }
                    }
                    if (questFishing.maxCount > 0 && questFishing.count >= questFishing.maxCount)
                    {
                        questFishing.isComplete = true;
                    }
                }
                _instance?.RefreshOtherTaskList();
            }

            // 2. Bò Mộng (Daily Quest)
            if (npcTempId == 47 || npcTempId == 84 || text.Contains("Bò Mộng") || text.Contains("Số nhiệm vụ còn lại trong ngày:"))
            {
                if (text.Contains("Nhiệm vụ hiện tại:"))
                {
                    questBoMong.hasQuest = true;
                    string[] lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string l = lines[i].Trim();
                        if (l.StartsWith("Nhiệm vụ hiện tại:"))
                        {
                            questBoMong.title = l.Replace("Nhiệm vụ hiện tại:", "").Trim();
                            questBoMong.goal = questBoMong.title;
                        }
                        else if (l.Contains("Hiện tại đã hoàn thành:"))
                        {
                            string p = l.Replace("Hiện tại đã hoàn thành:", "").Trim();
                            int slash = p.IndexOf('/');
                            int openParen = p.IndexOf('(');
                            if (slash > 0 && openParen > slash)
                            {
                                string cStr = p.Substring(0, slash).Trim();
                                string mStr = p.Substring(slash + 1, openParen - slash - 1).Trim();
                                int.TryParse(cStr, out questBoMong.count);
                                int.TryParse(mStr, out questBoMong.maxCount);
                            }
                        }
                    }
                    questBoMong.isComplete = (questBoMong.maxCount > 0 && questBoMong.count >= questBoMong.maxCount);
                }
                else if (text.Contains("Tôi có vài nhiệm vụ theo cấp bậc"))
                {
                    questBoMong.hasQuest = false;
                    questBoMong.isComplete = false;
                    questBoMong.count = 0;
                    questBoMong.maxCount = 0;
                    questBoMong.title = "Nhiệm vụ hằng ngày";
                    questBoMong.goal = "Chưa nhận nhiệm vụ hằng ngày từ Bò Mộng.";
                }
                _instance?.RefreshOtherTaskList();
            }

            // 3. Kanao (Vô Hạn Thành)
            if (npcTempId == 112 || text.Contains("Vô Hạn Thành") || text.Contains("Kanao"))
            {
                if (text.Contains("Bạn chưa có nhiệm vụ Vô Hạn Thành"))
                {
                    questKanao.hasQuest = false;
                    questKanao.isComplete = false;
                    questKanao.count = 0;
                    questKanao.maxCount = 0;
                    questKanao.title = "Nhiệm vụ Vô Hạn Thành";
                    questKanao.goal = "Chưa nhận nhiệm vụ Vô Hạn Thành từ Kanao.";
                }
                else if (text.Contains("Nhiệm vụ Vô Hạn Thành") && text.Contains("Tiến độ:"))
                {
                    questKanao.hasQuest = true;
                    questKanao.title = "Nhiệm vụ Vô Hạn Thành";
                    string[] lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string l = lines[i].Trim();
                        if (l.StartsWith("Tiêu diệt"))
                        {
                            questKanao.goal = l;
                        }
                        else if (l.Contains("Tiến độ:"))
                        {
                            string p = l.Replace("Tiến độ:", "").Trim();
                            string[] parts = p.Split('/');
                            if (parts.Length == 2)
                            {
                                int.TryParse(parts[0].Trim(), out questKanao.count);
                                int.TryParse(parts[1].Trim(), out questKanao.maxCount);
                            }
                        }
                    }
                    questKanao.isComplete = (questKanao.maxCount > 0 && questKanao.count >= questKanao.maxCount);
                }
                _instance?.RefreshOtherTaskList();
            }

            // 4. Bang hội (Clan)
            if (text.Contains("Nhiệm vụ hiện tại:") && text.Contains("Đã hạ được"))
            {
                questClan.hasQuest = true;
                string[] lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    string l = lines[i].Trim();
                    if (l.StartsWith("Nhiệm vụ hiện tại:"))
                    {
                        string payload = l.Replace("Nhiệm vụ hiện tại:", "").Trim();
                        int progressMarker = payload.IndexOf(". Đã hạ được", StringComparison.OrdinalIgnoreCase);
                        if (progressMarker >= 0)
                        {
                            questClan.title = payload.Substring(0, progressMarker).Trim();
                            int.TryParse(payload.Substring(progressMarker + 13).Trim(), out questClan.count);
                        }
                        else questClan.title = payload;
                        questClan.goal = questClan.title;
                        questClan.maxCount = FindFirstPositiveInt(questClan.title);
                        questClan.isComplete = questClan.maxCount > 0 && questClan.count >= questClan.maxCount;
                    }
                }
                _instance?.RefreshOtherTaskList();
            }
        }

        public static void OnThongBao(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            // Fishing events
            if (text.Contains("Đã nhận phần thưởng nhiệm vụ câu cá") || text.Contains("Đã xóa nhiệm vụ mức"))
            {
                questFishing.hasQuest = false;
                questFishing.isComplete = false;
                questFishing.count = 0;
                questFishing.maxCount = 0;
                questFishing.title = "Nhiệm vụ câu cá";
                questFishing.goal = "Chưa nhận nhiệm vụ câu cá.";
                _instance?.RefreshOtherTaskList();
            }
            else if (text.StartsWith("Đã nhận nhiệm vụ"))
            {
                questFishing.hasQuest = true;
                questFishing.isComplete = false;
                questFishing.count = 0;
                int colon = text.IndexOf(':');
                if (colon > 0)
                {
                    questFishing.title = text.Substring(0, colon).Trim();
                    questFishing.goal = text.Substring(colon + 1).Trim().TrimEnd('.');
                }
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("Nhiệm vụ câu cá") && text.Contains("hoàn thành"))
            {
                questFishing.isComplete = true;
                if (questFishing.maxCount > 0) questFishing.count = questFishing.maxCount;
                _instance?.RefreshOtherTaskList();
            }

            // Clan progress messages start with "Nhiệm vụ:". Keep them out of
            // the daily quest bucket, which uses "Nhiệm vụ <tên> ...".
            if (text.StartsWith("Nhiệm vụ:") && text.Contains("đã hoàn thành:"))
            {
                ApplyProgressMessage(questClan, text, "Nhiệm vụ:");
                _instance?.RefreshOtherTaskList();
                return;
            }

            // BoMong events
            if (text.StartsWith("Bạn nhận được nhiệm vụ:"))
            {
                questBoMong.hasQuest = true;
                questBoMong.isComplete = false;
                questBoMong.count = 0;
                questBoMong.title = text.Replace("Bạn nhận được nhiệm vụ:", "").Trim();
                questBoMong.goal = questBoMong.title;
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("hủy bỏ nhiệm vụ"))
            {
                questBoMong.hasQuest = false;
                questBoMong.isComplete = false;
                questBoMong.count = 0;
                questBoMong.maxCount = 0;
                questBoMong.title = "Nhiệm vụ hằng ngày";
                questBoMong.goal = "Chưa nhận nhiệm vụ hằng ngày từ Bò Mộng.";
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("đã hoàn thành:") && text.Contains("%"))
            {
                ApplyProgressMessage(questBoMong, text, "Nhiệm vụ");
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("quay về Bò Mộng trả nhiệm vụ"))
            {
                questBoMong.isComplete = true;
                if (questBoMong.maxCount > 0) questBoMong.count = questBoMong.maxCount;
                _instance?.RefreshOtherTaskList();
            }

            // Kanao events
            if (text.StartsWith("Nhiệm vụ Kanao:"))
            {
                questKanao.hasQuest = true;
                string p = text.Replace("Nhiệm vụ Kanao:", "").Trim().TrimEnd('.');
                string[] parts = p.Split('/');
                if (parts.Length == 2)
                {
                    int.TryParse(parts[0].Trim(), out questKanao.count);
                    int.TryParse(parts[1].Trim(), out questKanao.maxCount);
                }
                if (questKanao.maxCount > 0 && questKanao.count >= questKanao.maxCount)
                {
                    questKanao.isComplete = true;
                }
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("Đã hoàn thành nhiệm vụ Kanao"))
            {
                questKanao.hasQuest = true;
                questKanao.isComplete = true;
                if (questKanao.maxCount > 0) questKanao.count = questKanao.maxCount;
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("nhận thưởng nhiệm vụ Kanao"))
            {
                questKanao.hasQuest = false;
                questKanao.isComplete = false;
                questKanao.count = 0;
                questKanao.maxCount = 0;
                questKanao.title = "Nhiệm vụ Vô Hạn Thành";
                questKanao.goal = "Chưa nhận nhiệm vụ Vô Hạn Thành từ Kanao.";
                _instance?.RefreshOtherTaskList();
            }

            // Clan events
            if (text.Contains("Đã hủy nhiệm vụ bang"))
            {
                questClan.hasQuest = false;
                questClan.isComplete = false;
                questClan.count = 0;
                questClan.maxCount = 0;
                questClan.title = "Nhiệm vụ bang hội";
                questClan.goal = "Chưa nhận nhiệm vụ bang hội.";
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("Tiếp theo hãy về Bang hội báo cáo"))
            {
                questClan.isComplete = true;
                _instance?.RefreshOtherTaskList();
            }
        }

        public void RefreshOtherTaskList()
        {
            ConfigureScrollAdapters();
        }

        private void Init()
        {
            _inputContext = new UiInputContext();
            _selectedMainTab = 0;
            _selectedSubTab = 0;
            _keyboardFocus = KeyboardFocusContent;
            EnsureAssets();

            int screenW = GameCanvas.w;
            int screenH = GameCanvas.h;

            int frameW = System.Math.Min(MaxFrameWidth, screenW - 16);
            int frameH = System.Math.Min(MaxFrameHeight, screenH - 16);
            if (frameW < 360) frameW = screenW - 8;
            if (frameH < 220) frameH = screenH - 8;

            int frameX = (screenW - frameW) / 2;
            int frameY = (screenH - frameH) / 2;
            _frameRect = new UiRect(frameX, frameY, frameW, frameH);

            int contentX = frameX + SidebarWidth;
            int contentW = frameW - SidebarWidth;
            int contentH = frameH - FooterHeight;

            _footerRect = new UiRect(contentX, frameY + contentH, contentW, FooterHeight);
            _closeBtnRect = new UiRect(_footerRect.X + _footerRect.Width - 24, _footerRect.Y + 2, 22, 22);

            _tabBarRect = new UiRect(frameX, frameY, SidebarWidth, frameH);
            _contentRect = new UiRect(contentX, frameY, contentW, contentH);

            int columnWidth = (contentW - 12) / 2;
            int leftX = contentX + 4;
            int subTabW = (columnWidth - 4) / 2;
            _subTab0Rect = new UiRect(leftX, frameY + 4, subTabW, 22);
            _subTab1Rect = new UiRect(_subTab0Rect.X + subTabW + 4, frameY + 4, subTabW, 22);
            _leftColRect = new UiRect(leftX, frameY + 31, columnWidth, contentH - 34);

            int rightX = leftX + columnWidth + 4;
            _rightColRect = new UiRect(rightX, frameY + 4, columnWidth, contentH - 8);
            _rightBodyRect = new UiRect(_rightColRect.X + 2, _rightColRect.Y + 26, _rightColRect.Width - 4, _rightColRect.Height - 28);

            // Auto-select active main task
            Task currentTask = (Char.myCharz() != null) ? Char.myCharz().taskMaint : null;
            if (currentTask != null)
            {
                _selectedTaskPosition = GetMainTaskPosition((int)currentTask.taskId);
            }

            SelectFirstAvailableOtherQuest();
            ConfigureScrollAdapters();
            _leftScrollAdapter?.ScrollToIndex(_selectedTaskPosition);
        }

        private static void EnsureAssets()
        {
            if (_mainTabIcons == null)
            {
                _mainTabIcons = new Image[]
                {
                    GameCanvas.loadImage("/custom_menu/tab_task.png"),
                    GameCanvas.loadImage("/custom_menu/tab_inventory.png"),
                    GameCanvas.loadImage("/custom_menu/tab_skill.png"),
                    GameCanvas.loadImage("/custom_menu/tab_clan.png"),
                    GameCanvas.loadImage("/custom_menu/tab_function.png")
                };
            }
            if (_otherTaskIcons == null)
            {
                _otherTaskIcons = new Image[]
                {
                    GameCanvas.loadImage("/custom_menu/task_daily.png"),
                    GameCanvas.loadImage("/custom_menu/task_infinity.png"),
                    GameCanvas.loadImage("/custom_menu/task_fishing.png"),
                    GameCanvas.loadImage("/custom_menu/task_clan.png"),
                    GameCanvas.loadImage("/custom_menu/task_other.png")
                };
            }
            if (_statusIcons == null)
            {
                _statusIcons = new Image[]
                {
                    GameCanvas.loadImage("/custom_menu/status_tick.png"),
                    GameCanvas.loadImage("/custom_menu/status_lock.png")
                };
            }
        }

        private static void ApplyProgressMessage(NpcQuest quest, string text, string titlePrefix)
        {
            int marker = text.IndexOf("đã hoàn thành:", StringComparison.OrdinalIgnoreCase);
            if (marker <= 0) return;
            quest.hasQuest = true;
            string title = text.Substring(0, marker).Trim().TrimEnd('.');
            if (title.StartsWith(titlePrefix)) title = title.Substring(titlePrefix.Length).TrimStart(':', ' ');
            if (!string.IsNullOrEmpty(title))
            {
                quest.title = title;
                quest.goal = title;
            }
            string progress = text.Substring(marker + 14).Trim();
            int slash = progress.IndexOf('/');
            int end = progress.IndexOf('(');
            if (slash > 0)
            {
                int.TryParse(progress.Substring(0, slash).Trim(), out quest.count);
                string max = end > slash ? progress.Substring(slash + 1, end - slash - 1) : progress.Substring(slash + 1);
                int.TryParse(max.Trim(), out quest.maxCount);
            }
            quest.isComplete = quest.maxCount > 0 && quest.count >= quest.maxCount;
        }

        private static int FindFirstPositiveInt(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int value = 0;
            bool reading = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c >= '0' && c <= '9')
                {
                    reading = true;
                    value = value * 10 + c - '0';
                }
                else if (reading) return value;
            }
            return value;
        }

        private static int GetCurrentTaskId()
        {
            Task task = Char.myCharz() != null ? Char.myCharz().taskMaint : null;
            return task != null ? (int)task.taskId : 0;
        }

        private static int GetMainTaskPosition(int taskId)
        {
            if (taskId <= 3) return System.Math.Max(0, taskId);
            if (taskId <= 6) return 4;
            return taskId - 2;
        }

        private static int[] BuildMainTaskSequence()
        {
            int gender = Char.myCharz() != null ? Char.myCharz().cgender : 0;
            if (gender < 0 || gender > 2) gender = 0;
            int maxTaskId = System.Math.Max(LastKnownMainTaskId, GetCurrentTaskId() + 1);
            if (maxTaskId >= CanonicalTaskSubSteps.Length) maxTaskId = CanonicalTaskSubSteps.Length - 1;
            if (maxTaskId < 7) maxTaskId = 7;

            int[] result = new int[maxTaskId - 1];
            result[0] = 0;
            result[1] = 1;
            result[2] = 2;
            result[3] = 3;
            result[4] = 4 + gender;
            int position = 5;
            for (int taskId = 7; taskId <= maxTaskId; taskId++) result[position++] = taskId;
            return result;
        }

        private int GetSelectedTaskId()
        {
            int[] sequence = BuildMainTaskSequence();
            if (_selectedTaskPosition < 0) _selectedTaskPosition = 0;
            if (_selectedTaskPosition >= sequence.Length) _selectedTaskPosition = sequence.Length - 1;
            return sequence[_selectedTaskPosition];
        }

        private void SelectFirstAvailableOtherQuest()
        {
            if (_selectedOtherCategoryIndex >= 0 && _selectedOtherCategoryIndex < allOtherQuests.Length && allOtherQuests[_selectedOtherCategoryIndex].hasQuest) return;
            int[] order = new int[] { 0, 1, 3, 2 };
            for (int i = 0; i < order.Length; i++)
            {
                if (allOtherQuests[order[i]].hasQuest)
                {
                    _selectedOtherCategoryIndex = order[i];
                    return;
                }
            }
            _selectedOtherCategoryIndex = 0;
        }

        private void ConfigureScrollAdapters()
        {
            if (_leftScrollAdapter == null) _leftScrollAdapter = new ScrollViewAdapter();
            if (_rightScrollAdapter == null) _rightScrollAdapter = new ScrollViewAdapter();

            if (_selectedSubTab == 0)
            {
                int rowCount = GetMainTaskRowCount();
                _leftScrollAdapter.Configure(GetLeftListViewport(), rowCount, MainTaskRowHeight);
                int rightH = GetRightContentHeight();
                _rightScrollAdapter.Configure(_rightBodyRect, (rightH / 10) + 1, 10);
            }
            else
            {
                int totalH = GetOtherListHeight();
                _leftScrollAdapter.Configure(_leftColRect, (totalH / 10) + 1, 10);
                int rightH = GetRightContentHeight();
                _rightScrollAdapter.Configure(_rightBodyRect, (rightH / 10) + 1, 10);
            }
        }

        private int GetMainTaskRowCount()
        {
            return BuildMainTaskSequence().Length;
        }

        private UiRect GetLeftListViewport()
        {
            return _leftColRect;
        }

        private int GetOtherListHeight()
        {
            int h = 0;
            h += 14 + ((questBoMong.hasQuest ? 1 : 0) + (questKanao.hasQuest ? 1 : 0)) * (OtherTaskRowHeight + 1);
            if (!questBoMong.hasQuest && !questKanao.hasQuest) h += 21;
            h += 4 + 14 + (questClan.hasQuest ? OtherTaskRowHeight + 1 : 21);
            h += 4 + 14 + (questFishing.hasQuest ? OtherTaskRowHeight + 1 : 21);
            return h + 2;
        }

        private int GetRightContentHeight()
        {
            if (_selectedMainTab != 0) return 100;

            if (_selectedSubTab == 0)
            {
                int taskId = GetSelectedTaskId();
                int currentTaskPosition = GetMainTaskPosition(GetCurrentTaskId());
                if (_selectedTaskPosition > currentTaskPosition) return 62;
                int h = 27;
                string[] steps = GetTaskSteps(taskId);
                for (int i = 0; i < steps.Length; i++) h += mFont.tahoma_7b_dark.splitFontArray(steps[i], _rightBodyRect.Width - 30).Length * 14 + 2;
                string description = GetMainTaskGuide(taskId);
                if (!string.IsNullOrEmpty(description)) h += mFont.tahoma_7b_dark.splitFontArray(description, _rightBodyRect.Width - 18).Length * 14 + 7;
                string[] rewards = GetTaskRewards(taskId);
                if (rewards != null) h += 24 + rewards.Length * 15;
                return h + 12;
            }
            else
            {
                NpcQuest q = allOtherQuests[_selectedOtherCategoryIndex];
                int h = 38;
                if (q.hasQuest)
                {
                    h += mFont.tahoma_7b_dark.splitFontArray(q.goal ?? string.Empty, _rightBodyRect.Width - 30).Length * 14 + 2;
                    h += mFont.tahoma_7b_dark.splitFontArray(q.summary ?? string.Empty, _rightBodyRect.Width - 30).Length * 14 + 7;
                }
                else h += 18;
                h += 25 + mFont.tahoma_7b_dark.splitFontArray(q.defaultHint ?? string.Empty, _rightBodyRect.Width - 18).Length * 14;
                return h + 8;
            }
        }

        private static string GetMainTaskName(int taskId)
        {
            Task currentTask = Char.myCharz() != null ? Char.myCharz().taskMaint : null;
            if (currentTask != null && (int)currentTask.taskId == taskId && currentTask.names != null && currentTask.names.Length > 0)
            {
                string liveName = string.Join(" ", currentTask.names).Trim();
                if (!string.IsNullOrEmpty(liveName)) return liveName;
            }
            if (taskId >= 0 && taskId < MainTaskNames.Length) return MainTaskNames[taskId];
            return "Nhiệm vụ";
        }

        private static string[] GetTaskSteps(int taskId)
        {
            Task currentTask = Char.myCharz() != null ? Char.myCharz().taskMaint : null;
            if (currentTask != null && (int)currentTask.taskId == taskId && currentTask.subNames != null && currentTask.subNames.Length > 0)
                return currentTask.subNames;
            if (taskId >= 0 && taskId < CanonicalTaskSubSteps.Length && CanonicalTaskSubSteps[taskId] != null)
                return CanonicalTaskSubSteps[taskId];
            return new string[0];
        }

        private static string GetMainTaskGuide(int taskId)
        {
            Task currentTask = Char.myCharz() != null ? Char.myCharz().taskMaint : null;
            if (currentTask != null && (int)currentTask.taskId == taskId && currentTask.details != null)
            {
                string result = string.Empty;
                for (int i = 0; i < currentTask.details.Length; i++)
                {
                    string line = currentTask.details[i];
                    if (string.IsNullOrEmpty(line) || line.IndexOf("thưởng", StringComparison.OrdinalIgnoreCase) >= 0) break;
                    if (result.Length > 0) result += " ";
                    result += line.Trim();
                }
                if (!string.IsNullOrEmpty(result) && !string.Equals(result, "Chi tiết nhiệm vụ", StringComparison.OrdinalIgnoreCase)) return result;
            }
            if (taskId >= 0 && taskId < CanonicalTaskGuides.Length) return CanonicalTaskGuides[taskId];
            return string.Empty;
        }

        private static string[] GetTaskRewards(int taskId)
        {
            Task currentTask = Char.myCharz() != null ? Char.myCharz().taskMaint : null;
            if (currentTask != null && (int)currentTask.taskId == taskId && currentTask.details != null)
            {
                List<string> rewards = new List<string>();
                for (int i = 0; i < currentTask.details.Length; i++)
                {
                    string line = currentTask.details[i] ?? string.Empty;
                    int marker = line.IndexOf("thưởng", StringComparison.OrdinalIgnoreCase);
                    if (marker < 0) continue;
                    string reward = line.Substring(marker + 6).Trim().TrimStart('-', ':', ' ');
                    if (!string.IsNullOrEmpty(reward)) rewards.Add(reward);
                }
                if (rewards.Count > 0) return rewards.ToArray();
            }
            if (taskId >= 0 && taskId < CanonicalTaskRewards.Length && CanonicalTaskRewards[taskId] != null)
                return CanonicalTaskRewards[taskId];
            return new string[] { "Vàng và tiềm năng" };
        }

        public override void update()
        {
            base.update();
            _leftScrollAdapter?.Update();
            _rightScrollAdapter?.Update();
        }

        public override void updateKey()
        {
            if (_inputContext == null) return;

            // 1. ESC or Back key
            if (GameCanvas.keyAsciiPress == 27 || GameCanvas.keyPressed[12] || GameCanvas.keyPressed[13])
            {
                GameCanvas.keyAsciiPress = 0;
                GameCanvas.clearKeyPressed();
                Close();
                return;
            }

            // 2. F7 key
            if (GameCanvas.keyAsciiPress == -27 || GameCanvas.keyPressed[17])
            {
                GameCanvas.keyAsciiPress = 0;
                GameCanvas.clearKeyPressed();
                Close();
                return;
            }

            // 3. Hierarchical keyboard navigation between sidebar, subtabs, and rows.
            if (HandleKeyboardNavigation()) return;

            // Pointer on Close Button
            if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_closeBtnRect.X, _closeBtnRect.Y, _closeBtnRect.Width, _closeBtnRect.Height))
            {
                GameCanvas.isPointerJustRelease = false;
                Close();
                return;
            }

            // Pointer on Vertical Tab Bar
            if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_tabBarRect.X, _tabBarRect.Y, _tabBarRect.Width, _tabBarRect.Height))
            {
                int clickedTab = (GameCanvas.py - _tabBarRect.Y) * MainTabCount / _tabBarRect.Height;
                if (clickedTab >= 0 && clickedTab < MainTabCount)
                {
                    _selectedMainTab = clickedTab;
                    if (_selectedMainTab == 0) _selectedSubTab = 0;
                    _keyboardFocus = KeyboardFocusMainTabs;
                    GameCanvas.isPointerJustRelease = false;
                    _leftScrollAdapter?.Reset();
                    _rightScrollAdapter?.Reset();
                    ConfigureScrollAdapters();
                    _leftScrollAdapter?.ScrollToIndex(_selectedTaskPosition);
                    return;
                }
            }

            if (_selectedMainTab == 0)
            {
                // Pointer on Sub-Tabs
                if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_subTab0Rect.X, _subTab0Rect.Y, _subTab0Rect.Width, _subTab0Rect.Height))
                {
                    _selectedSubTab = 0;
                    _keyboardFocus = KeyboardFocusContent;
                    GameCanvas.isPointerJustRelease = false;
                    _leftScrollAdapter?.Reset();
                    _rightScrollAdapter?.Reset();
                    ConfigureScrollAdapters();
                    _leftScrollAdapter?.ScrollToIndex(_selectedTaskPosition);
                    return;
                }

                if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_subTab1Rect.X, _subTab1Rect.Y, _subTab1Rect.Width, _subTab1Rect.Height))
                {
                    _selectedSubTab = 1;
                    _keyboardFocus = KeyboardFocusContent;
                    GameCanvas.isPointerJustRelease = false;
                    _leftScrollAdapter?.Reset();
                    _rightScrollAdapter?.Reset();
                    ConfigureScrollAdapters();
                    return;
                }

                // Left Column Scroll and Click
                if (_selectedSubTab == 0 && _leftScrollAdapter != null && _leftScrollAdapter.UpdateKey(_inputContext, out int clickedIndex))
                {
                    if (clickedIndex >= 0 && clickedIndex < GetMainTaskRowCount())
                    {
                        _selectedTaskPosition = clickedIndex;
                        _keyboardFocus = KeyboardFocusContent;
                    }
                    _rightScrollAdapter?.Reset();
                    ConfigureScrollAdapters();
                    return;
                }

                // Pointer Click on Left Column (Nhiệm vụ khác)
                if (_selectedSubTab == 1 && GameCanvas.isPointerJustRelease)
                {
                    for (int i = 0; i < _otherQuestCardRects.Length; i++)
                    {
                        UiRect rect = _otherQuestCardRects[i];
                        if (rect.Width > 0 && GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height))
                        {
                            _selectedOtherCategoryIndex = i;
                            _keyboardFocus = KeyboardFocusContent;
                            GameCanvas.isPointerJustRelease = false;
                            _rightScrollAdapter?.Reset();
                            ConfigureScrollAdapters();
                            return;
                        }
                    }
                }

                // Right Column Scroll
                if (_rightScrollAdapter != null && _rightScrollAdapter.UpdateKey(_inputContext, out _))
                {
                    return;
                }
            }

            base.updateKey();
        }

        public override void paint(mGraphics g)
        {
            g.translate(-g.getTranslateX(), -g.getTranslateY());
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h + 1);

            // 1. Drop shadow for window
            g.setColor(0x000000);
            g.fillRect(_frameRect.X + 3, _frameRect.Y + 3, _frameRect.Width, _frameRect.Height);

            // 2. Main Frame Background
            g.setColor(0xECE0CF); // Warm cream fill
            g.fillRect(_frameRect.X, _frameRect.Y, _frameRect.Width, _frameRect.Height);

            // Frame border
            g.setColor(0x5C4638);
            g.drawRect(_frameRect.X, _frameRect.Y, _frameRect.Width, _frameRect.Height);

            // 3. Left Vertical Tab Bar
            PaintVerticalTabBar(g);

            // 4. Footer Bar
            PaintFooterBar(g);

            // 5. Content Area
            if (_selectedMainTab == 0)
            {
                PaintTaskTabContent(g);
            }
            else
            {
                PaintEmptyTabContent(g);
            }
        }

        private void PaintVerticalTabBar(mGraphics g)
        {
            g.setColor(0xEAE5DC);
            g.fillRect(_tabBarRect.X, _tabBarRect.Y, _tabBarRect.Width, _tabBarRect.Height);
            g.setColor(0x3E3A34);
            g.drawRect(_tabBarRect.X, _tabBarRect.Y, _tabBarRect.Width, _tabBarRect.Height);

            for (int i = 0; i < MainTabCount; i++)
            {
                int ty = _tabBarRect.Y + i * _tabBarRect.Height / MainTabCount;
                int nextY = _tabBarRect.Y + (i + 1) * _tabBarRect.Height / MainTabCount;
                int tabH = nextY - ty;
                bool isSelected = (i == _selectedMainTab);
                if (isSelected)
                {
                    g.setColor(0xFFCB35);
                    g.fillRect(_tabBarRect.X + 1, ty + 1, _tabBarRect.Width - 1, tabH - 1);
                    if (_keyboardFocus == KeyboardFocusMainTabs)
                    {
                        g.setColor(0xFFF2A8);
                        g.drawRect(_tabBarRect.X + 2, ty + 2, _tabBarRect.Width - 5, tabH - 5);
                    }
                }
                g.setColor(0x766B5D);
                g.drawLine(_tabBarRect.X, ty + tabH, _tabBarRect.X + _tabBarRect.Width, ty + tabH);
                Image icon = _mainTabIcons != null && i < _mainTabIcons.Length ? _mainTabIcons[i] : null;
                if (icon != null)
                {
                    g.drawImage(icon, _tabBarRect.X + _tabBarRect.Width / 2, ty + tabH / 2, mGraphics.HCENTER | mGraphics.VCENTER);
                }
            }
        }

        private void PaintFooterBar(mGraphics g)
        {
            g.setColor(0xE9E4DA);
            g.fillRect(_footerRect.X, _footerRect.Y, _footerRect.Width, _footerRect.Height);
            g.setColor(0x7B6F60);
            g.drawRect(_footerRect.X, _footerRect.Y, _footerRect.Width, _footerRect.Height);

            Char me = Char.myCharz();
            long xu = me != null ? me.xu : 0L;
            int luong = me != null ? me.luong : 0;
            int luongKhoa = me != null ? me.luongKhoa : 0;
            int usableW = _footerRect.Width - 28;
            PaintCurrency(g, _footerRect.X + usableW / 6, Panel.imgXu, NinjaUtil.getMoneys(xu));
            PaintCurrency(g, _footerRect.X + usableW / 2, Panel.imgLuong, NinjaUtil.getMoneys(luong));
            PaintCurrency(g, _footerRect.X + usableW * 5 / 6, Panel.imgLuongKhoa, NinjaUtil.getMoneys(luongKhoa));

            g.setColor(0xF7C400);
            g.fillRect(_closeBtnRect.X, _closeBtnRect.Y, _closeBtnRect.Width, _closeBtnRect.Height);
            g.setColor(0xE59600);
            g.drawRect(_closeBtnRect.X, _closeBtnRect.Y, _closeBtnRect.Width, _closeBtnRect.Height);
            g.setColor(0xE35A24);
            g.drawLine(_closeBtnRect.X + 5, _closeBtnRect.Y + 5, _closeBtnRect.X + 17, _closeBtnRect.Y + 17);
            g.drawLine(_closeBtnRect.X + 6, _closeBtnRect.Y + 5, _closeBtnRect.X + 18, _closeBtnRect.Y + 17);
            g.drawLine(_closeBtnRect.X + 17, _closeBtnRect.Y + 5, _closeBtnRect.X + 5, _closeBtnRect.Y + 17);
            g.drawLine(_closeBtnRect.X + 18, _closeBtnRect.Y + 5, _closeBtnRect.X + 6, _closeBtnRect.Y + 17);
        }

        private void PaintCurrency(mGraphics g, int centerX, Image icon, string value)
        {
            int textW = mFont.tahoma_7_orange.getWidth(value);
            int iconW = icon != null ? icon.getWidth() : 14;
            int startX = centerX - (iconW + 5 + textW) / 2;
            if (icon != null) g.drawImage(icon, startX + iconW / 2, _footerRect.Y + _footerRect.Height / 2, mGraphics.HCENTER | mGraphics.VCENTER);
            mFont.tahoma_7_orange.drawString(g, value, startX + iconW + 5, _footerRect.Y + 7, mFont.LEFT);
        }

        private void PaintEmptyTabContent(mGraphics g)
        {
            int midX = _contentRect.X + _contentRect.Width / 2;
            int midY = _contentRect.Y + _contentRect.Height / 2;

            g.setColor(0xDFD2BC);
            g.fillRect(_contentRect.X + 4, _contentRect.Y + 4, _contentRect.Width - 8, _contentRect.Height - 8);
            g.setColor(0xC4B79B);
            g.drawRect(_contentRect.X + 4, _contentRect.Y + 4, _contentRect.Width - 8, _contentRect.Height - 8);

            string tabName = MainTabNames[_selectedMainTab];
            mFont.tahoma_7b_dark.drawString(g, tabName, midX, midY - 14, mFont.CENTER);
            mFont.tahoma_7_grey.drawString(g, "Chức năng đang được phát triển...", midX, midY + 4, mFont.CENTER);
        }

        private void PaintTaskTabContent(mGraphics g)
        {
            PaintSubTabs(g);
            PaintTaskListColumn(g);
            PaintTaskDetailColumn(g);
        }

        private void PaintSubTabs(mGraphics g)
        {
            // SubTab 0: Nhiệm vụ chính
            bool isTab0 = (_selectedSubTab == 0);
            g.setColor(isTab0 ? 0x63BE00 : 0xE99A00);
            g.fillRect(_subTab0Rect.X, _subTab0Rect.Y, _subTab0Rect.Width, _subTab0Rect.Height);
            g.setColor(isTab0 ? 0x527D00 : 0xA56800);
            g.drawRect(_subTab0Rect.X, _subTab0Rect.Y, _subTab0Rect.Width, _subTab0Rect.Height);
            mFont.tahoma_7b_dark.drawString(g, "Nhiệm vụ chính", _subTab0Rect.X + _subTab0Rect.Width / 2, _subTab0Rect.Y + 4, mFont.CENTER);

            // SubTab 1: Nhiệm vụ khác
            bool isTab1 = (_selectedSubTab == 1);
            g.setColor(isTab1 ? 0x63BE00 : 0xE99A00);
            g.fillRect(_subTab1Rect.X, _subTab1Rect.Y, _subTab1Rect.Width, _subTab1Rect.Height);
            g.setColor(isTab1 ? 0x527D00 : 0xA56800);
            g.drawRect(_subTab1Rect.X, _subTab1Rect.Y, _subTab1Rect.Width, _subTab1Rect.Height);
            mFont.tahoma_7b_dark.drawString(g, "Nhiệm vụ khác", _subTab1Rect.X + _subTab1Rect.Width / 2, _subTab1Rect.Y + 4, mFont.CENTER);
        }

        private void PaintTaskListColumn(mGraphics g)
        {
            UiRect listViewport = GetLeftListViewport();
            g.setColor(0xC4B79B);
            g.drawRect(listViewport.X, listViewport.Y, listViewport.Width, listViewport.Height);

            using (UiRenderState.Push(g, listViewport, clip: true))
            {
                g.setColor(0xDFD2BC);
                g.fillRect(_leftColRect.X, _leftColRect.Y, _leftColRect.Width, _leftColRect.Height);

                int scrollY = (_leftScrollAdapter != null) ? _leftScrollAdapter.ScrollY : 0;
                int clipTop = _leftColRect.Y;
                int clipBottom = listViewport.Y + listViewport.Height;

                if (_selectedSubTab == 0)
                {
                    // Main Tasks
                    Task currentTask = (Char.myCharz() != null) ? Char.myCharz().taskMaint : null;
                    int currentTaskId = currentTask != null ? (int)currentTask.taskId : 0;
                    int currentTaskPosition = GetMainTaskPosition(currentTaskId);
                    int[] taskSequence = BuildMainTaskSequence();
                    int totalRows = GetMainTaskRowCount();
                    int numberColW = 34;
                    int maxTextW = _leftColRect.Width - numberColW - 35;
                    int textX = _leftColRect.X + numberColW + 6;

                    for (int i = 0; i < totalRows; i++)
                    {
                        int rowY = _leftColRect.Y + i * MainTaskRowHeight - scrollY;
                        if (rowY + MainTaskRowHeight <= clipTop || rowY >= clipBottom) continue;

                        bool isSelected = (i == _selectedTaskPosition);
                        int status;
                        if (i < currentTaskPosition) status = 0;
                        else if (i == currentTaskPosition) status = 1;
                        else status = 2;

                        // Row background
                        g.setColor(isSelected ? 0xFFD038 : (status == 2 ? 0xD8D8D8 : 0xF2F0ED));
                        g.fillRect(_leftColRect.X + 1, rowY, _leftColRect.Width - 2, MainTaskRowHeight - 1);

                        g.setColor(status == 2 ? 0xC4B9A8 : 0xB8A58B);
                        g.fillRect(_leftColRect.X + 1, rowY, numberColW, MainTaskRowHeight - 1);
                        mFont.tahoma_7b_dark.drawString(g, (i + 1).ToString(), _leftColRect.X + numberColW / 2, rowY + 8, mFont.CENTER);

                        // Divider line
                        g.setColor(0xC4B79B);
                        int divY = rowY + MainTaskRowHeight - 1;
                        if (divY >= clipTop && divY < clipBottom)
                        {
                            g.drawLine(_leftColRect.X + 1, divY, _leftColRect.X + _leftColRect.Width - 2, divY);
                        }

                        // Task Title
                        int taskId = taskSequence[i];
                        string taskTitle = GetMainTaskName(taskId);

                        if (rowY + 4 >= clipTop && rowY + 4 < clipBottom)
                        {
                            string displayTitle = TruncateString(mFont.tahoma_7b_dark, taskTitle, maxTextW);
                            mFont.tahoma_7b_dark.drawString(g, displayTitle, textX, rowY + 3, mFont.LEFT);
                        }

                        // Status text & icons with strict bounds checking
                        bool iconVisible = (rowY + 6 >= clipTop && rowY + 26 <= clipBottom);
                        bool statusTextVisible = (rowY + 18 >= clipTop && rowY + 18 < clipBottom);

                        if (status == 0) // Done
                        {
                            if (statusTextVisible)
                                mFont.tahoma_7_green2.drawString(g, "Đã hoàn thành", textX, rowY + 18, mFont.LEFT);
                            if (iconVisible)
                            {
                                Image statusIcon = _statusIcons != null && _statusIcons.Length > 0 ? _statusIcons[0] : null;
                                if (statusIcon != null) g.drawImage(statusIcon, _leftColRect.X + _leftColRect.Width - 12, rowY + MainTaskRowHeight / 2, mGraphics.HCENTER | mGraphics.VCENTER);
                                else DrawCheckmark(g, _leftColRect.X + _leftColRect.Width - 20, rowY + 10, 14);
                            }
                        }
                        else if (status == 1) // In Progress
                        {
                            if (statusTextVisible)
                            {
                                mFont progressFont = isSelected ? mFont.tahoma_7b_dark : mFont.tahoma_7_orange;
                                progressFont.drawString(g, "Đang thực hiện", textX, rowY + 17, mFont.LEFT);
                            }
                            if (rowY + 12 >= clipTop && rowY + 12 < clipBottom)
                            {
                                int pct = 0;
                                if (currentTask != null && currentTask.counts != null && currentTask.index < currentTask.counts.Length && currentTask.counts[currentTask.index] > 0)
                                {
                                    pct = currentTask.count * 100 / currentTask.counts[currentTask.index];
                                }
                                if (pct < 0) pct = 0;
                                if (pct > 100) pct = 100;
                                string pctStr = pct + "%";
                                mFont.tahoma_7b_dark.drawString(g, pctStr, _leftColRect.X + _leftColRect.Width - 8, rowY + 17, mFont.RIGHT);
                            }
                        }
                        else // Locked
                        {
                            if (statusTextVisible)
                                mFont.tahoma_7_grey.drawString(g, "Khóa", textX, rowY + 18, mFont.LEFT);
                            if (iconVisible)
                            {
                                Image statusIcon = _statusIcons != null && _statusIcons.Length > 1 ? _statusIcons[1] : null;
                                if (statusIcon != null) g.drawImage(statusIcon, _leftColRect.X + _leftColRect.Width - 12, rowY + MainTaskRowHeight / 2, mGraphics.HCENTER | mGraphics.VCENTER);
                                else DrawLock(g, _leftColRect.X + _leftColRect.Width - 20, rowY + 10, 11, 14);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _otherQuestCardRects.Length; i++) _otherQuestCardRects[i] = UiRect.Empty;
                    int groupY = _leftColRect.Y - scrollY;
                    groupY = PaintOtherQuestGroup(g, "Nhiệm vụ hằng ngày", new int[] { 0, 1 }, groupY);
                    groupY = PaintOtherQuestGroup(g, "Nhiệm vụ bang", new int[] { 3 }, groupY + 4);
                    PaintOtherQuestGroup(g, "Nhiệm vụ câu cá", new int[] { 2 }, groupY + 4);
                }
            }
        }

        private int PaintOtherQuestGroup(mGraphics g, string title, int[] questIndexes, int y)
        {
            mFont.tahoma_7b_dark.drawString(g, title, _leftColRect.X + 4, y, mFont.LEFT);
            y += 14;
            bool hasAny = false;
            for (int i = 0; i < questIndexes.Length; i++)
            {
                int questIndex = questIndexes[i];
                if (!allOtherQuests[questIndex].hasQuest) continue;
                PaintOtherQuestCard(g, questIndex, y);
                y += OtherTaskRowHeight + 1;
                hasAny = true;
            }
            if (!hasAny)
            {
                mFont.tahoma_7_grey.drawString(g, "--- Không có nhiệm vụ ---", _leftColRect.X + _leftColRect.Width / 2, y + 3, mFont.CENTER);
                y += 21;
            }
            return y;
        }

        private void PaintOtherQuestCard(mGraphics g, int questIndex, int y)
        {
            NpcQuest q = allOtherQuests[questIndex];
            UiRect rect = new UiRect(_leftColRect.X + 1, y, _leftColRect.Width - 2, OtherTaskRowHeight);
            _otherQuestCardRects[questIndex] = rect;
            bool selected = questIndex == _selectedOtherCategoryIndex;
            g.setColor(selected ? 0xFFD038 : 0xF2F0ED);
            g.fillRect(rect.X, rect.Y, rect.Width, rect.Height);
            g.setColor(0xC7B79F);
            g.drawRect(rect.X, rect.Y, rect.Width, rect.Height);

            Image icon = _otherTaskIcons != null && q.iconType < _otherTaskIcons.Length ? _otherTaskIcons[q.iconType] : null;
            if (icon != null) g.drawImage(icon, rect.X + 17, rect.Y + rect.Height / 2, mGraphics.HCENTER | mGraphics.VCENTER);
            int textX = rect.X + 36;
            string displayTitle = TruncateString(mFont.tahoma_7b_dark, q.title, rect.Width - 74);
            mFont.tahoma_7b_dark.drawString(g, displayTitle, textX, rect.Y + 3, mFont.LEFT);
            (q.isComplete ? mFont.tahoma_7_green2 : mFont.tahoma_7_orange).drawString(g, q.isComplete ? "Đã hoàn thành" : "Đang thực hiện", textX, rect.Y + 17, mFont.LEFT);
            int percent = q.isComplete ? 100 : (q.maxCount > 0 ? q.count * 100 / q.maxCount : 0);
            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;
            mFont.tahoma_7b_dark.drawString(g, percent + "%", rect.X + rect.Width - 7, rect.Y + 10, mFont.RIGHT);
        }

        private void PaintTaskDetailColumn(mGraphics g)
        {
            g.setColor(0xDFCFB7);
            g.fillRect(_rightColRect.X, _rightColRect.Y, _rightColRect.Width, _rightColRect.Height);
            g.setColor(0xB89261);
            g.drawRect(_rightColRect.X, _rightColRect.Y, _rightColRect.Width, _rightColRect.Height);

            g.setColor(0xEEE7DC);
            g.fillRect(_rightColRect.X + 2, _rightColRect.Y + 1, _rightColRect.Width - 4, 21);
            g.setColor(0xB8A58B);
            g.drawRect(_rightColRect.X + 2, _rightColRect.Y + 1, _rightColRect.Width - 4, 21);
            mFont.tahoma_7_orange.drawString(g, "Chi tiết nhiệm vụ", _rightColRect.X + _rightColRect.Width / 2, _rightColRect.Y + 5, mFont.CENTER);

            using (UiRenderState.Push(g, _rightBodyRect, clip: true))
            {
                int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
                int y = _rightBodyRect.Y + 2 - scrollY;
                if (_selectedSubTab == 0) PaintMainTaskDetailBody(g, y);
                else PaintOtherTaskDetailBody(g, y);
            }
        }

        private void PaintMainTaskDetailBody(mGraphics g, int y)
        {
            int taskId = GetSelectedTaskId();
            int currentTaskPosition = GetMainTaskPosition(GetCurrentTaskId());
            int status = _selectedTaskPosition < currentTaskPosition ? 0 : (_selectedTaskPosition == currentTaskPosition ? 1 : 2);
            Task currentTask = Char.myCharz() != null ? Char.myCharz().taskMaint : null;
            int currentStep = currentTask != null && (int)currentTask.taskId == taskId ? currentTask.index : 0;
            int midX = _rightBodyRect.X + _rightBodyRect.Width / 2;
            int bulletX = _rightBodyRect.X + 5;
            int textX = bulletX + 10;
            int maxTextW = _rightBodyRect.Width - 20;

            mFont.tahoma_7b_dark.drawString(g, GetMainTaskName(taskId), midX, y, mFont.CENTER);
            y += 18;
            DrawDetailDivider(g, y);
            y += 7;

            if (status == 2)
            {
                DrawWrappedText(g, mFont.tahoma_7b_dark, "Vui lòng hoàn thành nhiệm vụ trước đó để xem", _rightBodyRect.X + 4, y, _rightBodyRect.Width - 8);
                return;
            }

            string[] steps = GetTaskSteps(taskId);
            for (int i = 0; i < steps.Length; i++)
            {
                int color = 0x888888;
                mFont font = mFont.tahoma_7_grey;
                if (status == 0 || (status == 1 && i < currentStep))
                {
                    color = 0x00B86B;
                    font = mFont.tahoma_7_green2;
                }
                else if (status == 1 && i == currentStep)
                {
                    color = 0x496BFF;
                    font = mFont.tahoma_7_blue;
                }

                string step = steps[i] ?? string.Empty;
                if (status == 1 && i == currentStep && currentTask != null && currentTask.counts != null && i < currentTask.counts.Length && currentTask.counts[i] > 1)
                    step += " (" + currentTask.count + "/" + currentTask.counts[i] + ")";
                DrawBulletPoint(g, bulletX, y + 4, color);
                y = DrawWrappedText(g, font, step, textX, y, maxTextW);
                y += 2;
            }

            string description = GetMainTaskGuide(taskId);
            if (!string.IsNullOrEmpty(description))
            {
                y += 3;
                y = DrawWrappedText(g, mFont.tahoma_7b_dark, description, _rightBodyRect.X + 4, y, _rightBodyRect.Width - 8);
            }

            y += 5;
            DrawDetailDivider(g, y);
            y += 7;
            mFont.tahoma_7b_red.drawString(g, "Phần thưởng :", _rightBodyRect.X + 4, y, mFont.LEFT);
            y += 16;
            string[] rewards = GetTaskRewards(taskId);
            for (int i = 0; i < rewards.Length; i++)
            {
                mFont.tahoma_7b_red.drawString(g, rewards[i], _rightBodyRect.X + 4, y, mFont.LEFT);
                y += 15;
            }
        }

        private void PaintOtherTaskDetailBody(mGraphics g, int y)
        {
            NpcQuest q = allOtherQuests[_selectedOtherCategoryIndex];
            int midX = _rightBodyRect.X + _rightBodyRect.Width / 2;
            int bulletX = _rightBodyRect.X + 5;
            int textX = bulletX + 10;
            int maxTextW = _rightBodyRect.Width - 20;

            mFont.tahoma_7b_dark.drawString(g, q.title, midX, y, mFont.CENTER);
            y += 18;
            DrawDetailDivider(g, y);
            y += 7;

            if (q.hasQuest)
            {
                int color = q.isComplete ? 0x00B86B : 0x496BFF;
                mFont goalFont = q.isComplete ? mFont.tahoma_7_green2 : mFont.tahoma_7_blue;
                string goal = q.goal ?? string.Empty;
                if (q.maxCount > 0) goal += " (" + q.count + "/" + q.maxCount + ")";
                DrawBulletPoint(g, bulletX, y + 4, color);
                y = DrawWrappedText(g, goalFont, goal, textX, y, maxTextW);
                y += 2;
                DrawBulletPoint(g, bulletX, y + 4, 0x00B86B);
                y = DrawWrappedText(g, mFont.tahoma_7_green2, q.isComplete ? "Quay lại gặp " + q.npcName + " để hoàn tất nhiệm vụ." : q.summary, textX, y, maxTextW);
            }
            else
            {
                y = DrawWrappedText(g, mFont.tahoma_7_grey, "Chưa có nhiệm vụ.", _rightBodyRect.X + 4, y, _rightBodyRect.Width - 8);
            }

            y += 7;
            DrawDetailDivider(g, y);
            y += 7;
            mFont.tahoma_7b_dark.drawString(g, "Hướng dẫn :", _rightBodyRect.X + 4, y, mFont.LEFT);
            y += 16;
            DrawWrappedText(g, mFont.tahoma_7b_dark, q.defaultHint, _rightBodyRect.X + 4, y, _rightBodyRect.Width - 8);
        }

        private int DrawWrappedText(mGraphics g, mFont font, string text, int x, int y, int width)
        {
            string[] lines = font.splitFontArray(text ?? string.Empty, width);
            for (int i = 0; i < lines.Length; i++)
            {
                font.drawString(g, lines[i], x, y, mFont.LEFT);
                y += 14;
            }
            return y;
        }

        private void DrawDetailDivider(mGraphics g, int y)
        {
            g.setColor(0xB89261);
            g.drawLine(_rightBodyRect.X + 3, y, _rightBodyRect.X + _rightBodyRect.Width - 4, y);
        }

        private void PaintLegacyTaskDetailColumn(mGraphics g)
        {
            g.setColor(0xC4B79B);
            g.drawRect(_rightColRect.X, _rightColRect.Y, _rightColRect.Width, _rightColRect.Height);

            using (UiRenderState.Push(g, _rightColRect, clip: true))
            {
                g.setColor(0xDFD2BC);
                g.fillRect(_rightColRect.X, _rightColRect.Y, _rightColRect.Width, _rightColRect.Height);

                int scrollY = (_rightScrollAdapter != null) ? _rightScrollAdapter.ScrollY : 0;
                int curY = _rightColRect.Y + 8 - scrollY;
                int midX = _rightColRect.X + _rightColRect.Width / 2;
                int textX = _rightColRect.X + 18;
                int bulletX = _rightColRect.X + 8;
                int maxBulletW = _rightColRect.Width - 28;

                if (_selectedSubTab == 0)
                {
                    // ---------------- MAIN TASK DETAILS ----------------
                    Task currentTask = (Char.myCharz() != null) ? Char.myCharz().taskMaint : null;
                    int currentTaskId = (currentTask != null) ? (int)currentTask.taskId : 0;
                    int currentStepIndex = (currentTask != null) ? currentTask.index : 0;

                    int status;
                    if (_selectedTaskPosition < currentTaskId) status = 0;      // Hoàn thành
                    else if (_selectedTaskPosition == currentTaskId) status = 1; // Đang làm
                    else status = 2;                                          // Khóa

                    string taskTitle = (_selectedTaskPosition >= 0 && _selectedTaskPosition < MainTaskNames.Length)
                        ? MainTaskNames[_selectedTaskPosition]
                        : ("Nhiệm vụ số " + (_selectedTaskPosition + 1));

                    // Title Header
                    mFont.tahoma_7b_dark.drawString(g, taskTitle, midX, curY, mFont.CENTER);
                    curY += 16;

                    // Divider
                    g.setColor(0xD3C4A8);
                    g.drawLine(_rightColRect.X + 8, curY, _rightColRect.X + _rightColRect.Width - 8, curY);
                    curY += 8;

                    // Detail description for current task
                    if (_selectedTaskPosition == currentTaskId && currentTask != null && currentTask.details != null && currentTask.details.Length > 0)
                    {
                        string rawDetail = string.Join(" ", currentTask.details);
                        string[] detailLines = mFont.tahoma_7b_dark.splitFontArray(rawDetail, maxBulletW);
                        for (int l = 0; l < detailLines.Length; l++)
                        {
                            mFont.tahoma_7b_dark.drawString(g, detailLines[l], _rightColRect.X + 8, curY, mFont.LEFT);
                            curY += 14;
                        }
                        curY += 6;
                    }

                    // Section: Các bước thực hiện
                    mFont.tahoma_7b_dark.drawString(g, "Các bước thực hiện :", _rightColRect.X + 8, curY, mFont.LEFT);
                    curY += 16;

                    string[] subSteps = (_selectedTaskPosition >= 0 && _selectedTaskPosition < CanonicalTaskSubSteps.Length)
                        ? CanonicalTaskSubSteps[_selectedTaskPosition]
                        : null;

                    if (subSteps != null && subSteps.Length > 0)
                    {
                        for (int s = 0; s < subSteps.Length; s++)
                        {
                            string stepText = subSteps[s];
                            int stepDotColor;
                            mFont stepFont;

                            if (status == 0) // Completed
                            {
                                stepDotColor = 0x27AE60; // Green
                                stepFont = mFont.tahoma_7_green2;
                            }
                            else if (status == 1) // In progress
                            {
                                if (s < currentStepIndex)
                                {
                                    stepDotColor = 0x27AE60; // Green done
                                    stepFont = mFont.tahoma_7_green2;
                                }
                                else if (s == currentStepIndex)
                                {
                                    stepDotColor = 0x2980B9; // Blue active
                                    stepFont = mFont.tahoma_7_blue;
                                    if (currentTask.counts != null && s < currentTask.counts.Length && currentTask.counts[s] > 0)
                                    {
                                        stepText += " (" + currentTask.count + "/" + currentTask.counts[s] + ")";
                                    }
                                }
                                else
                                {
                                    stepDotColor = 0x888888; // Gray upcoming
                                    stepFont = mFont.tahoma_7_grey;
                                }
                            }
                            else // Locked
                            {
                                stepDotColor = 0x888888;
                                stepFont = mFont.tahoma_7_grey;
                            }

                            DrawBulletPoint(g, bulletX, curY + 4, stepDotColor);

                            string[] stepLines = stepFont.splitFontArray(stepText, maxBulletW);
                            for (int sl = 0; sl < stepLines.Length; sl++)
                            {
                                stepFont.drawString(g, stepLines[sl], textX, curY, mFont.LEFT);
                                curY += 14;
                            }
                            curY += 2;
                        }
                    }
                    else
                    {
                        mFont.tahoma_7_grey.drawString(g, "• Chưa có dữ liệu bước làm", textX, curY, mFont.LEFT);
                        curY += 15;
                    }

                    curY += 4;
                    g.setColor(0xD3C4A8);
                    g.drawLine(_rightColRect.X + 8, curY, _rightColRect.X + _rightColRect.Width - 8, curY);
                    curY += 8;

                    // Section: Phần thưởng
                    mFont.tahoma_7b_red.drawString(g, "Phần thưởng :", _rightColRect.X + 8, curY, mFont.LEFT);
                    curY += 16;

                    string[] rewards = (_selectedTaskPosition >= 0 && _selectedTaskPosition < CanonicalTaskRewards.Length)
                        ? CanonicalTaskRewards[_selectedTaskPosition]
                        : null;

                    if (rewards != null && rewards.Length > 0)
                    {
                        for (int r = 0; r < rewards.Length; r++)
                        {
                            DrawBulletPoint(g, bulletX, curY + 4, 0xC0392B);
                            mFont.tahoma_7b_red.drawString(g, rewards[r], textX, curY, mFont.LEFT);
                            curY += 15;
                        }
                    }
                    else
                    {
                        DrawBulletPoint(g, bulletX, curY + 4, 0xC0392B);
                        mFont.tahoma_7b_red.drawString(g, "Vàng và Tiềm năng", textX, curY, mFont.LEFT);
                        curY += 15;
                    }
                }
                else
                {
                    // ---------------- OTHER TASK DETAILS ----------------
                    if (_selectedOtherCategoryIndex >= 0 && _selectedOtherCategoryIndex < allOtherQuests.Length)
                    {
                        NpcQuest q = allOtherQuests[_selectedOtherCategoryIndex];

                        // Header: Category Name (NPC)
                        mFont.tahoma_7b_dark.drawString(g, q.categoryName + " (" + q.npcName + ")", midX, curY, mFont.CENTER);
                        curY += 16;

                        // Divider
                        g.setColor(0xD3C4A8);
                        g.drawLine(_rightColRect.X + 8, curY, _rightColRect.X + _rightColRect.Width - 8, curY);
                        curY += 8;

                        // Section 1: Mục tiêu
                        mFont.tahoma_7b_dark.drawString(g, "Mục tiêu :", _rightColRect.X + 8, curY, mFont.LEFT);
                        curY += 16;

                        if (q.hasQuest)
                        {
                            int dotColor = q.isComplete ? 0x27AE60 : 0x2980B9;
                            mFont statusFont = q.isComplete ? mFont.tahoma_7_green2 : mFont.tahoma_7_blue;

                            DrawBulletPoint(g, bulletX, curY + 4, dotColor);
                            string goalStr = q.goal;
                            if (q.maxCount > 0 && !goalStr.Contains("(" + q.count + "/"))
                            {
                                goalStr += " (" + q.count + "/" + q.maxCount + ")";
                            }
                            string[] goalLines = statusFont.splitFontArray(goalStr, maxBulletW);
                            for (int l = 0; l < goalLines.Length; l++)
                            {
                                statusFont.drawString(g, goalLines[l], textX, curY, mFont.LEFT);
                                curY += 14;
                            }
                            curY += 2;

                            // Sub-status note
                            DrawBulletPoint(g, bulletX, curY + 4, dotColor);
                            string noteStr = q.isComplete ? "Đã hoàn thành! Hãy quay về gặp " + q.npcName + " để nhận thưởng." : "Đang thực hiện nhiệm vụ.";
                            statusFont.drawString(g, noteStr, textX, curY, mFont.LEFT);
                            curY += 16;
                        }
                        else
                        {
                            DrawBulletPoint(g, bulletX, curY + 4, 0x888888);
                            mFont.tahoma_7_grey.drawString(g, "Chưa nhận nhiệm vụ từ NPC " + q.npcName + ".", textX, curY, mFont.LEFT);
                            curY += 18;
                        }

                        // Divider
                        g.setColor(0xD3C4A8);
                        g.drawLine(_rightColRect.X + 8, curY, _rightColRect.X + _rightColRect.Width - 8, curY);
                        curY += 8;

                        // Section 2: Hướng dẫn nhận & làm nhiệm vụ
                        mFont.tahoma_7b_dark.drawString(g, "Hướng dẫn nhận & làm nhiệm vụ :", _rightColRect.X + 8, curY, mFont.LEFT);
                        curY += 16;

                        if (!string.IsNullOrEmpty(q.defaultHint))
                        {
                            string[] hintLines = mFont.tahoma_7b_dark.splitFontArray(q.defaultHint, _rightColRect.Width - 20);
                            for (int h = 0; h < hintLines.Length; h++)
                            {
                                mFont.tahoma_7b_dark.drawString(g, hintLines[h], _rightColRect.X + 10, curY, mFont.LEFT);
                                curY += 14;
                            }
                            curY += 6;
                        }

                        // Divider
                        g.setColor(0xD3C4A8);
                        g.drawLine(_rightColRect.X + 8, curY, _rightColRect.X + _rightColRect.Width - 8, curY);
                        curY += 8;

                        // Section 3: Ghi chú
                        mFont.tahoma_7_grey.drawString(g, "• Tự động đồng bộ với máy chủ khi nhận, hoàn thành hoặc hủy bỏ.", _rightColRect.X + 8, curY, mFont.LEFT);
                    }
                }
            }
        }

        private string TruncateString(mFont font, string text, int maxWidth)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (font.getWidth(text) <= maxWidth) return text;
            string s = text;
            while (s.Length > 0 && font.getWidth(s + "...") > maxWidth)
            {
                s = s.Substring(0, s.Length - 1);
            }
            return s + "...";
        }

        // Icons
        private void DrawBullIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x8D6E63);
            g.fillRect(x + 4, y + 6, 12, 10);
            g.setColor(0xD7CCC8);
            g.fillRect(x + 2, y + 2, 4, 5);
            g.fillRect(x + 14, y + 2, 4, 5);
            g.setColor(0xBCAAA4);
            g.fillRect(x + 6, y + 11, 8, 5);
            g.setColor(0x3E2723);
            g.fillRect(x + 6, y + 8, 2, 2);
            g.fillRect(x + 12, y + 8, 2, 2);
        }

        private void DrawKanaoIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x9B59B6);
            g.fillRect(x + 3, y + 4, 6, 6);
            g.fillRect(x + 11, y + 4, 6, 6);
            g.fillRect(x + 4, y + 10, 5, 5);
            g.fillRect(x + 11, y + 10, 5, 5);
            g.setColor(0x2C3E50);
            g.fillRect(x + 9, y + 3, 2, 12);
        }

        private void DrawFishIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x3498DB);
            g.fillRect(x + 4, y + 6, 10, 6);
            g.setColor(0x2980B9);
            g.fillRect(x + 2, y + 7, 2, 4);
            g.fillRect(x + 14, y + 4, 3, 10);
            g.setColor(0xFFFFFF);
            g.fillRect(x + 5, y + 7, 2, 2);
        }

        private void DrawCheckmark(mGraphics g, int x, int y, int size)
        {
            g.setColor(0x27AE60);
            int cx = x + 3;
            int cy = y + size / 2;
            for (int i = 0; i < 3; i++)
            {
                g.drawLine(cx + i, cy + i, cx + i + 1, cy + i + 1);
            }
            for (int i = 0; i < 7; i++)
            {
                g.drawLine(cx + 3 + i, cy + 3 - i, cx + 4 + i, cy + 3 - i);
            }
        }

        private void DrawLock(mGraphics g, int x, int y, int w, int h)
        {
            g.setColor(0x7F8C8D);
            g.drawRect(x + 2, y + 1, w - 4, h / 2);
            g.fillRect(x, y + h / 2, w, h / 2);
            g.setColor(0x2C3E50);
            g.fillRect(x + w / 2 - 1, y + h / 2 + 2, 2, 3);
        }

        private void DrawBulletPoint(mGraphics g, int x, int y, int color)
        {
            g.setColor(color);
            g.fillRect(x, y, 3, 3);
        }

        private void DrawScrollIcon(mGraphics g, int x, int y)
        {
            g.setColor(0xF9E79F);
            g.fillRect(x + 5, y + 3, 16, 18);
            g.setColor(0xD4AC0D);
            g.drawRect(x + 5, y + 3, 16, 18);
            g.setColor(0xB7950B);
            g.fillRect(x + 3, y + 2, 4, 20);
            g.fillRect(x + 19, y + 2, 4, 20);
            g.setColor(0x7D6608);
            g.drawLine(x + 8, y + 8, x + 18, y + 8);
            g.drawLine(x + 8, y + 12, x + 18, y + 12);
            g.drawLine(x + 8, y + 16, x + 15, y + 16);
        }

        private void DrawBackpackIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x8B4513);
            g.fillRect(x + 4, y + 6, 18, 15);
            g.setColor(0x5C2D0C);
            g.drawRect(x + 4, y + 6, 18, 15);
            g.setColor(0xA0522D);
            g.fillRect(x + 4, y + 4, 18, 6);
            g.setColor(0xF1C40F);
            g.fillRect(x + 11, y + 9, 4, 3);
            g.setColor(0x5C2D0C);
            g.drawLine(x + 9, y + 3, x + 17, y + 3);
            g.drawLine(x + 9, y + 3, x + 9, y + 5);
            g.drawLine(x + 17, y + 3, x + 17, y + 5);
        }

        private void DrawBookIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x2471A3);
            g.fillRect(x + 5, y + 3, 16, 18);
            g.setColor(0x1B4F72);
            g.drawRect(x + 5, y + 3, 16, 18);
            g.setColor(0x5499C7);
            g.fillRect(x + 3, y + 3, 4, 18);
            g.setColor(0xF1C40F);
            g.fillRect(x + 10, y + 8, 7, 7);
            g.setColor(0x2471A3);
            g.fillRect(x + 12, y + 10, 3, 3);
        }

        private void DrawShieldIcon(mGraphics g, int x, int y)
        {
            g.setColor(0xC0392B);
            g.fillRect(x + 5, y + 4, 16, 12);
            for (int i = 0; i < 6; i++)
            {
                g.drawLine(x + 5 + i, y + 16 + i, x + 20 - i, y + 16 + i);
            }
            g.setColor(0x78281F);
            g.drawRect(x + 5, y + 4, 16, 12);
            g.setColor(0xF1C40F);
            g.fillRect(x + 10, y + 8, 6, 6);
        }

        private void DrawGearIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x7F8C8D);
            g.fillRect(x + 7, y + 7, 12, 12);
            g.setColor(0x34495E);
            g.drawRect(x + 7, y + 7, 12, 12);
            g.setColor(0xD5C7B0);
            g.fillRect(x + 11, y + 11, 4, 4);
            g.setColor(0x7F8C8D);
            g.fillRect(x + 11, y + 4, 4, 3);
            g.fillRect(x + 11, y + 19, 4, 3);
            g.fillRect(x + 4, y + 11, 3, 4);
            g.fillRect(x + 19, y + 11, 3, 4);
        }
    }
}
