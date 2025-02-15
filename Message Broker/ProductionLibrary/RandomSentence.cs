namespace ProductionLibrary;

//a simple class for creating limited meaningful sentences
//because our messages has MessageId we do not worry about creating a sentence twice!

public class RandomSentence
{
    private static string[] Subjects = { "The cat", "A dog", "The teacher", "A child", "The car" };
    private static string[] Verbs = { "jumps", "runs", "sleeps", "eats", "drives" };
    private static string[] Objects = { "on the mat", "in the park", "to school", "with joy", "quickly" };
    private static string[] Adjectives = { "happy", "lazy", "quick", "excited", "curious" };

    public static String GenerateRandomSentence()
    {
        Random random = new Random();
        string subject = Subjects[random.Next(Subjects.Length)];
        string verb = Verbs[random.Next(Verbs.Length)];
        string obj = Objects[random.Next(Objects.Length)];
        string adjective = Adjectives[random.Next(Adjectives.Length)];
        
        return $"{subject} {verb} {obj} and is feeling {adjective}.";;
    }
}