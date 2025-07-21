using Jackson.Ideas.Mock.Models;
using Jackson.Ideas.Mock.Services.Interfaces;

namespace Jackson.Ideas.Mock.Services.Mock;

public class MockDataService : IMockDataService
{
    private readonly List<BusinessIdeaScenario> _scenarios;

    public MockDataService()
    {
        _scenarios = GenerateMockScenarios();
    }

    public async Task<List<BusinessIdeaScenario>> GetAllScenariosAsync()
    {
        await Task.Delay(100); // Simulate async operation
        return _scenarios;
    }

    public async Task<BusinessIdeaScenario?> GetScenarioByIdAsync(string id)
    {
        await Task.Delay(50);
        return _scenarios.FirstOrDefault(s => s.Id == id);
    }

    public async Task<List<BusinessIdeaScenario>> GetScenariosByIndustryAsync(string industry)
    {
        await Task.Delay(100);
        return _scenarios.Where(s => s.Industry.Equals(industry, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private static List<BusinessIdeaScenario> GenerateMockScenarios()
    {
        return new List<BusinessIdeaScenario>
        {
            new BusinessIdeaScenario
            {
                Id = "eco-eats-001",
                Name = "EcoEats Delivery",
                Description = "Sustainable food delivery service focusing on eco-friendly packaging and local restaurants. Founded by environmental science graduate Sarah Chen, EcoEats addresses the massive packaging waste problem in food delivery. Our platform exclusively partners with restaurants committed to zero-waste practices, using biodegradable containers and electric vehicle delivery. We've developed a unique 'Green Score' rating system that helps customers make environmentally conscious dining choices while supporting local businesses that share our sustainability values.",
                Industry = "Food & Beverage",
                TargetMarket = "Environmentally conscious millennials and Gen Z urban professionals",
                EstimatedStartupCost = 150000,
                ProjectedRevenue = 500000,
                ViabilityScore = 85,
                MarketSize = "$2.4B",
                KeyChallenges = new List<string> { "High competition from established players", "Logistics complexity for sustainable packaging", "Higher operational costs for eco-friendly practices" },
                SuccessFactors = new List<string> { "Strong environmental brand identity", "Strategic local restaurant partnerships", "Robust technology platform with Green Score system" }
            },
            new BusinessIdeaScenario
            {
                Id = "ai-study-buddy-002",
                Name = "AI Study Buddy",
                Description = "AI-powered personalized tutoring platform for students of all ages. Created by former teacher Maria Rodriguez after witnessing the learning gaps in traditional education, AI Study Buddy uses advanced machine learning to adapt to each student's learning style, pace, and knowledge gaps. The platform provides real-time feedback, generates custom practice problems, and connects students with human tutors when needed. Our proprietary algorithm identifies learning patterns and optimizes study schedules for maximum retention and comprehension.",
                Industry = "Education Technology",
                TargetMarket = "Students ages 8-18, parents, and educational institutions",
                EstimatedStartupCost = 200000,
                ProjectedRevenue = 800000,
                ViabilityScore = 78,
                MarketSize = "$850M",
                KeyChallenges = new List<string> { "High AI development and maintenance costs", "Extensive content creation across subjects", "Competitive user acquisition in crowded market" },
                SuccessFactors = new List<string> { "Adaptive learning technology that truly personalizes", "High-quality educational content", "Strong user engagement and retention metrics" }
            },
            new BusinessIdeaScenario
            {
                Id = "localcraft-marketplace-003",
                Name = "LocalCraft Marketplace",
                Description = "Online marketplace connecting local artisans with customers seeking handmade goods. Founded by craft enthusiast and former retail manager James Patterson, this platform celebrates local creativity while providing artisans with digital marketing tools and streamlined order management. Unlike mass-market competitors, we focus exclusively on handmade, locally-sourced items, offering customers authenticity stories behind each product. Our community-driven approach includes artisan workshops, customer reviews, and a 'Local Heroes' feature highlighting craftspeople's journeys.",
                Industry = "E-commerce",
                TargetMarket = "Craft enthusiasts, conscious consumers, gift buyers seeking unique items",
                EstimatedStartupCost = 75000,
                ProjectedRevenue = 300000,
                ViabilityScore = 72,
                MarketSize = "$1.2B",
                KeyChallenges = new List<string> { "Building critical mass of both artisans and customers", "Maintaining quality standards across diverse products", "Complex shipping logistics for fragile handmade items" },
                SuccessFactors = new List<string> { "Strong community building and engagement", "Comprehensive artisan support and training", "Effective digital marketing and SEO strategy" }
            },
            new BusinessIdeaScenario
            {
                Id = "fitness-vr-004",
                Name = "FitVR Studios",
                Description = "Virtual reality fitness platform offering immersive workout experiences. Former video game developer and fitness enthusiast Alex Kim recognized the potential to gamify exercise through VR technology. FitVR Studios creates engaging fitness adventures where users can box with virtual trainers, explore exotic locations while running, or participate in group classes from home. Our platform tracks biometric data, provides personalized coaching, and builds social communities around shared fitness goals in virtual worlds.",
                Industry = "Health & Fitness Technology",
                TargetMarket = "Tech-savvy fitness enthusiasts, gamers, remote workers",
                EstimatedStartupCost = 300000,
                ProjectedRevenue = 1200000,
                ViabilityScore = 73,
                MarketSize = "$4.5B",
                KeyChallenges = new List<string> { "High VR hardware requirements for users", "Substantial content development costs", "Motion sickness and safety concerns" },
                SuccessFactors = new List<string> { "Compelling and diverse VR fitness content", "Strategic partnerships with VR hardware manufacturers", "Strong community features and social integration" }
            },
            new BusinessIdeaScenario
            {
                Id = "senior-tech-support-005",
                Name = "TechBridge Senior Support",
                Description = "Specialized technology support service for seniors navigating digital devices. Founded by gerontology student and IT professional Lisa Wang after helping her grandmother with smartphone frustrations, TechBridge provides patient, empathetic technology training for older adults. Our certified Senior Tech Advocates offer in-home visits, simplified tutorials, and ongoing support for everything from basic smartphone use to video calling with family. We focus on building confidence and independence in the digital world.",
                Industry = "Technology Services",
                TargetMarket = "Adults 65+, their adult children seeking tech help for parents",
                EstimatedStartupCost = 80000,
                ProjectedRevenue = 400000,
                ViabilityScore = 82,
                MarketSize = "$650M",
                KeyChallenges = new List<string> { "Building trust with technology-wary demographic", "Scaling personalized in-home service model", "Keeping pace with rapidly changing technology" },
                SuccessFactors = new List<string> { "Highly trained, patient, empathetic staff", "Simple, age-appropriate training materials", "Strong word-of-mouth referral system" }
            },
            new BusinessIdeaScenario
            {
                Id = "urban-farming-006",
                Name = "CityGrow Solutions",
                Description = "Urban vertical farming systems for restaurants and institutions. Agricultural engineer Dr. Priya Patel developed innovative hydroponic systems that allow restaurants to grow fresh herbs and vegetables on-site, reducing supply chain costs and ensuring peak freshness. CityGrow provides complete farming solutions including equipment, seeds, nutrients, and ongoing support. Our modular systems can be installed in basements, rooftops, or unused spaces, providing year-round fresh produce regardless of climate or season.",
                Industry = "Agriculture Technology",
                TargetMarket = "Restaurants, hotels, institutions, urban food producers",
                EstimatedStartupCost = 250000,
                ProjectedRevenue = 900000,
                ViabilityScore = 76,
                MarketSize = "$1.8B",
                KeyChallenges = new List<string> { "High initial equipment and setup costs", "Technical expertise required for system maintenance", "Competition from traditional produce suppliers" },
                SuccessFactors = new List<string> { "Proven ROI through reduced food costs", "Comprehensive training and support programs", "Strategic partnerships with restaurant chains" }
            },
            new BusinessIdeaScenario
            {
                Id = "mental-wellness-app-007",
                Name = "MindfulPath",
                Description = "AI-powered mental wellness platform combining therapy and self-care tools. Clinical psychologist Dr. Michael Torres created MindfulPath after recognizing the gap between traditional therapy and daily mental health support. Our platform provides personalized meditation programs, mood tracking, AI-powered check-ins, and connects users with licensed therapists when needed. The app uses evidence-based cognitive behavioral therapy techniques and adapts to each user's mental health journey and progress.",
                Industry = "Mental Health Technology",
                TargetMarket = "Adults seeking mental health support, therapy patients, wellness enthusiasts",
                EstimatedStartupCost = 180000,
                ProjectedRevenue = 750000,
                ViabilityScore = 79,
                MarketSize = "$3.2B",
                KeyChallenges = new List<string> { "Strict healthcare regulations and privacy requirements", "Building trust for sensitive mental health data", "Balancing AI automation with human therapeutic care" },
                SuccessFactors = new List<string> { "Clinical validation and evidence-based approaches", "Strong data privacy and security measures", "Integration with existing healthcare systems" }
            },
            new BusinessIdeaScenario
            {
                Id = "pet-care-concierge-008",
                Name = "PawConcierge",
                Description = "Comprehensive pet care service offering grooming, walking, training, and health monitoring. Founded by veterinary technician and dog trainer Amanda Foster, PawConcierge provides busy pet owners with reliable, professional care for their beloved animals. Our trained pet care specialists offer customized service packages including daily walks, grooming, basic training, medication administration, and health monitoring. We use a mobile app for real-time updates, photos, and communication with pet owners.",
                Industry = "Pet Services",
                TargetMarket = "Busy professionals with pets, elderly pet owners, frequent travelers",
                EstimatedStartupCost = 120000,
                ProjectedRevenue = 600000,
                ViabilityScore = 84,
                MarketSize = "$2.1B",
                KeyChallenges = new List<string> { "Building trust with pet owners for animal care", "Managing scheduling and logistics for multiple pets", "Insurance and liability considerations" },
                SuccessFactors = new List<string> { "Highly trained and bonded pet care staff", "Transparent communication and photo updates", "Strong local reputation and referral network" }
            },
            new BusinessIdeaScenario
            {
                Id = "smart-home-elderly-009",
                Name = "SafeHome Seniors",
                Description = "Smart home safety systems designed specifically for aging adults. Biomedical engineer David Park developed this comprehensive monitoring system after his father's fall went undetected for hours. SafeHome Seniors uses unobtrusive sensors, AI monitoring, and emergency response systems to help seniors live independently while providing peace of mind to family members. Our system detects falls, medication reminders, unusual activity patterns, and can automatically contact emergency services or family members when needed.",
                Industry = "Healthcare Technology",
                TargetMarket = "Seniors aging in place, adult children of elderly parents, assisted living facilities",
                EstimatedStartupCost = 220000,
                ProjectedRevenue = 850000,
                ViabilityScore = 77,
                MarketSize = "$1.5B",
                KeyChallenges = new List<string> { "Complex installation and setup requirements", "Privacy concerns with home monitoring", "Integration with existing emergency response systems" },
                SuccessFactors = new List<string> { "Reliable and accurate health monitoring", "Easy-to-use family communication portal", "Fast and effective emergency response protocols" }
            },
            new BusinessIdeaScenario
            {
                Id = "sustainable-fashion-010",
                Name = "ThreadCycle",
                Description = "Circular fashion platform for clothing rental, repair, and upcycling services. Fashion design student and sustainability advocate Emma Rodriguez created ThreadCycle to combat fast fashion's environmental impact. Our platform allows customers to rent designer clothing for special events, provides professional clothing repair and alteration services, and offers upcycling workshops to transform old garments into new fashion pieces. We partner with local designers and seamstresses to build a community-driven circular economy.",
                Industry = "Sustainable Fashion",
                TargetMarket = "Environmentally conscious fashion lovers, special event attendees, budget-conscious consumers",
                EstimatedStartupCost = 100000,
                ProjectedRevenue = 450000,
                ViabilityScore = 71,
                MarketSize = "$890M",
                KeyChallenges = new List<string> { "Inventory management for rental business", "Quality control for repairs and alterations", "Changing consumer mindset about clothing ownership" },
                SuccessFactors = new List<string> { "High-quality garment curation and care", "Skilled repair and upcycling artisans", "Strong brand identity around sustainability" }
            },
            new BusinessIdeaScenario
            {
                Id = "local-food-network-011",
                Name = "FarmTable Connect",
                Description = "Platform connecting local farms directly with restaurants and consumers. Former restaurant manager and agriculture advocate Carlos Martinez built FarmTable Connect to eliminate middlemen in the local food supply chain. Our platform enables restaurants to source fresh, seasonal ingredients directly from nearby farms while providing consumers access to farm-fresh produce through convenient pickup locations. We handle logistics, payments, and seasonal planning to make local sourcing simple and profitable for all parties.",
                Industry = "Food Technology",
                TargetMarket = "Local restaurants, conscious consumers, small-scale farmers",
                EstimatedStartupCost = 140000,
                ProjectedRevenue = 550000,
                ViabilityScore = 80,
                MarketSize = "$1.3B",
                KeyChallenges = new List<string> { "Complex logistics and seasonal supply management", "Building trust between farmers and restaurants", "Competing with established food distributors" },
                SuccessFactors = new List<string> { "Reliable logistics and delivery systems", "Strong relationships with both farms and restaurants", "Transparent pricing and quality standards" }
            },
            new BusinessIdeaScenario
            {
                Id = "language-immersion-vr-012",
                Name = "LinguaVerse",
                Description = "Virtual reality language learning through immersive cultural experiences. Language education specialist Dr. Kenji Nakamura developed LinguaVerse to address the lack of authentic cultural context in traditional language learning. Students can practice conversational skills with AI-powered native speakers in virtual Tokyo cafes, Parisian markets, or Mexican festivals. Our platform combines language instruction with cultural education, providing context and motivation that traditional apps can't match.",
                Industry = "Education Technology",
                TargetMarket = "Language learners, students, business professionals, travelers",
                EstimatedStartupCost = 280000,
                ProjectedRevenue = 950000,
                ViabilityScore = 74,
                MarketSize = "$2.1B",
                KeyChallenges = new List<string> { "High VR content development costs", "Need for VR headset adoption", "Competition from established language learning platforms" },
                SuccessFactors = new List<string> { "Immersive and culturally authentic content", "Advanced AI conversation partners", "Integration with existing language curricula" }
            },
            new BusinessIdeaScenario
            {
                Id = "elderly-social-network-013",
                Name = "WisdomCircle",
                Description = "Social networking platform designed specifically for seniors to share knowledge and connect. Former social worker and tech entrepreneur Helen Chang recognized that many social platforms alienate older adults with complex interfaces and irrelevant content. WisdomCircle provides a simplified, intuitive platform where seniors can share life experiences, mentor younger people, find local activities, and maintain meaningful connections. Our platform includes features like large text options, video calling tutorials, and community moderation focused on respectful interaction.",
                Industry = "Social Technology",
                TargetMarket = "Adults 60+, their families, intergenerational community builders",
                EstimatedStartupCost = 160000,
                ProjectedRevenue = 400000,
                ViabilityScore = 75,
                MarketSize = "$720M",
                KeyChallenges = new List<string> { "Overcoming technology adoption barriers", "Building critical mass of engaged users", "Monetization without alienating user base" },
                SuccessFactors = new List<string> { "Intuitive, age-friendly interface design", "Strong community moderation and safety features", "Valuable content and meaningful connections" }
            },
            new BusinessIdeaScenario
            {
                Id = "carbon-footprint-tracker-014",
                Name = "EcoTracker Pro",
                Description = "Comprehensive carbon footprint tracking and reduction platform for individuals and businesses. Environmental scientist Dr. Raj Patel created EcoTracker Pro to make carbon consciousness actionable and measurable. Our platform tracks daily activities, transportation, energy usage, and consumption patterns to provide detailed carbon footprint analysis. Users receive personalized recommendations for reduction strategies, can offset their footprint through verified projects, and compete with friends and colleagues in sustainability challenges.",
                Industry = "Environmental Technology",
                TargetMarket = "Environmentally conscious individuals, businesses seeking carbon neutrality, sustainability teams",
                EstimatedStartupCost = 190000,
                ProjectedRevenue = 700000,
                ViabilityScore = 76,
                MarketSize = "$1.4B",
                KeyChallenges = new List<string> { "Complex data integration from multiple sources", "Maintaining user engagement beyond initial enthusiasm", "Verifying accuracy of carbon offset projects" },
                SuccessFactors = new List<string> { "Accurate and comprehensive tracking capabilities", "Actionable and achievable reduction recommendations", "Transparent and verified offset marketplace" }
            },
            new BusinessIdeaScenario
            {
                Id = "skill-sharing-neighbors-015",
                Name = "NeighborSkills",
                Description = "Local skill-sharing platform connecting neighbors for teaching and learning. Community organizer and lifelong learner Rachel Green developed NeighborSkills to strengthen local communities while making learning accessible and affordable. Neighbors can offer to teach everything from cooking and gardening to financial planning and home repair. Our platform facilitates skill exchanges, schedules sessions, handles payments, and builds community connections through shared learning experiences.",
                Industry = "Community Technology",
                TargetMarket = "Community-minded individuals, lifelong learners, skilled professionals wanting to teach",
                EstimatedStartupCost = 90000,
                ProjectedRevenue = 350000,
                ViabilityScore = 78,
                MarketSize = "$650M",
                KeyChallenges = new List<string> { "Building trust between strangers for in-person meetings", "Ensuring quality and safety of skill instruction", "Creating sustainable monetization model" },
                SuccessFactors = new List<string> { "Strong vetting and review system for instructors", "Local community partnerships and promotion", "Diverse range of valuable skills offered" }
            },
            new BusinessIdeaScenario
            {
                Id = "remote-work-wellness-016",
                Name = "HomeOffice Wellness",
                Description = "Comprehensive wellness program designed specifically for remote workers. Occupational therapist and remote work consultant Dr. Nicole Brown created HomeOffice Wellness after recognizing the unique health challenges faced by distributed teams. Our platform provides ergonomic assessments, guided movement breaks, eye strain prevention, mental health check-ins, and virtual wellness sessions. We partner with employers to create healthier remote work environments and reduce healthcare costs.",
                Industry = "Corporate Wellness",
                TargetMarket = "Remote workers, distributed teams, HR departments, small businesses",
                EstimatedStartupCost = 130000,
                ProjectedRevenue = 520000,
                ViabilityScore = 81,
                MarketSize = "$1.1B",
                KeyChallenges = new List<string> { "Engaging employees in voluntary wellness programs", "Measuring and demonstrating ROI to employers", "Integrating with existing corporate health benefits" },
                SuccessFactors = new List<string> { "Evidence-based wellness interventions", "Easy integration with existing work tools", "Clear metrics showing productivity and health improvements" }
            },
            new BusinessIdeaScenario
            {
                Id = "artisan-food-subscription-017",
                Name = "CraftTaste",
                Description = "Curated subscription service featuring small-batch artisan foods from local producers. Former food critic and small business advocate Jennifer Lopez launched CraftTaste to support local food artisans while introducing consumers to unique, high-quality products. Each monthly box features carefully selected items from small producers, complete with stories about the makers, recipes, and pairing suggestions. We focus on sustainable, ethical producers who might not otherwise reach broader markets.",
                Industry = "Food Subscription",
                TargetMarket = "Food enthusiasts, conscious consumers, gift buyers, supporters of local business",
                EstimatedStartupCost = 110000,
                ProjectedRevenue = 480000,
                ViabilityScore = 79,
                MarketSize = "$950M",
                KeyChallenges = new List<string> { "Managing complex supply chain with multiple small producers", "Maintaining consistent quality across diverse products", "Customer acquisition in competitive subscription market" },
                SuccessFactors = new List<string> { "Strong curation and storytelling around products", "Reliable logistics and packaging for food items", "Building community around artisan food appreciation" }
            },
            new BusinessIdeaScenario
            {
                Id = "elderly-companion-robots-018",
                Name = "CompanionTech",
                Description = "AI-powered companion robots designed to provide emotional support and practical assistance for seniors. Robotics engineer and geriatric care specialist Dr. Robert Kim developed CompanionTech after witnessing his grandmother's isolation during the pandemic. Our robots provide conversation, medication reminders, emergency assistance, and entertainment while learning each user's preferences and routines. The robots can also facilitate video calls with family members and alert caregivers to changes in behavior or health patterns.",
                Industry = "Robotics & Healthcare",
                TargetMarket = "Seniors living independently, families with elderly relatives, assisted living facilities",
                EstimatedStartupCost = 400000,
                ProjectedRevenue = 1100000,
                ViabilityScore = 70,
                MarketSize = "$2.3B",
                KeyChallenges = new List<string> { "High development and manufacturing costs", "User acceptance of robotic companions", "Complex healthcare regulations and certifications" },
                SuccessFactors = new List<string> { "Advanced AI that creates genuine companionship", "Reliable health monitoring and emergency response", "Intuitive interface that seniors can easily use" }
            },
            new BusinessIdeaScenario
            {
                Id = "micro-learning-professionals-019",
                Name = "SkillSnap",
                Description = "Micro-learning platform delivering professional development in daily 5-minute sessions. Learning and development professional Kevin Zhang created SkillSnap to address the challenge of busy professionals finding time for skill development. Our platform delivers bite-sized lessons on leadership, communication, project management, and technical skills through various formats including video, interactive exercises, and quick quizzes. Content adapts to individual learning pace and professional goals.",
                Industry = "Professional Development",
                TargetMarket = "Working professionals, corporate training departments, career-focused individuals",
                EstimatedStartupCost = 170000,
                ProjectedRevenue = 650000,
                ViabilityScore = 83,
                MarketSize = "$1.6B",
                KeyChallenges = new List<string> { "Creating engaging content in very short formats", "Measuring learning effectiveness in micro-sessions", "Competing with established corporate training providers" },
                SuccessFactors = new List<string> { "High-quality, immediately applicable content", "Seamless integration into daily work routines", "Clear skill progression and achievement tracking" }
            },
            new BusinessIdeaScenario
            {
                Id = "community-solar-gardens-020",
                Name = "SolarShare Collective",
                Description = "Community solar garden platform enabling shared renewable energy ownership. Renewable energy engineer and environmental advocate Maria Santos developed SolarShare Collective to make solar energy accessible to renters and those unable to install personal solar systems. Participants can purchase or lease shares in local solar installations and receive credits on their electricity bills. Our platform handles all logistics, from project development to billing, making clean energy accessible to entire communities.",
                Industry = "Renewable Energy",
                TargetMarket = "Environmentally conscious consumers, renters, community organizations, local governments",
                EstimatedStartupCost = 350000,
                ProjectedRevenue = 1300000,
                ViabilityScore = 75,
                MarketSize = "$3.8B",
                KeyChallenges = new List<string> { "Complex regulatory approval processes", "High upfront capital requirements for solar installations", "Navigating utility company partnerships and grid integration" },
                SuccessFactors = new List<string> { "Strong relationships with utilities and regulators", "Transparent financial modeling and bill savings", "Community engagement and education programs" }
            }
        };
    }
}