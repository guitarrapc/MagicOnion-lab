using System;

namespace MagicOnionLab.Shared.Helpers
{
    public static class UserNameGenerator
    {
        private static readonly string[] names = new[]
        {
            "Emma", "Sophia", "Olivia", "Jacob", "Ava",
            "Emily", "Abigail", "Mia", "Isabella", "Madison",
            "Ethan", "Noah", "Daniel", "Aiden", "William",
            "Liam", "Jayden", "Alexander", "Michael", "Lucas",
            "Benjamin", "Mason", "Matthew", "Joshua", "Ryan",
            "Amelia", "Grace", "Charlotte", "Chloe", "Victoria",
            "Megan", "Rachel", "Sarah", "Jessica", "Samantha",
            "Ashley", "Nicole", "Daniel", "Ana", "Clara",
            "Carlos", "Roberto", "Pedro", "Maria", "Luis",
            "David", "Juan", "Diego", "Andrea", "Gabriela",
            "Patricia", "Sofia", "Ricardo", "Alejandro", "Antonio",
            "Miguel", "Javier", "Francisco", "Sergio", "Angel",
            "Raul", "Jose", "Fernanda", "Natalia", "Mariana",
            "Renata", "Camila", "Paola", "Fernando", "Eduardo",
            "Alfredo", "Manuel", "Jorge", "Rafa", "Adriana",
            "Alejandra", "Daniela", "Leticia", "Cristina", "Rosa",
            "Yoko", "Hiroshi", "Takeshi", "Yuki", "Naoki",
            "Satoshi", "Aiko", "Haruka", "Saki", "Keiko",
            "Toshiko", "Yoshio", "Hisao", "Hiroto", "Ryota",
            "Akane", "Yukiko", "Ayumi", "Wakana", "Kimiko"
        };
        private static readonly Random random = new Random();

        public static string GetRandomtName()
        {
            int PickIndex(int mod)
            {
                var r = random.Next();
                var i = r % mod;
                return i;
            }

            var index = PickIndex(names.Length);
            var name = names[index];
            return name;
        }
    }
}
