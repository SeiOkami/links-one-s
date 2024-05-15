
{
    const urlAuthorGithub = "https://github.com/SeiOkami";
    const urlRepository = urlAuthorGithub + "/links-one-s";
    const urlHowAdd = urlRepository + "#Как-добавить-канал";
    const urlIssues = urlRepository + "/issues";
    const urlAuthorTelegram = "https://t.me/SeiOkami";
    const urlJuniorOneS = "https://t.me/JuniorOneS";
    
    function openRepository(){
        openUrl(urlRepository);
    }

    function openHowAdd(){
        openUrl(urlHowAdd);
    }

    function openIssues(){
        openUrl(urlIssues);
    }

    function openAuthorGithub(){
        openUrl(urlAuthorGithub);
    }

    function openAuthorTelegram(){
        openUrl(urlAuthorTelegram);
    }

    function openJuniorOneS(){
        openUrl(urlJuniorOneS);
    }

    function openUrl(url){
        window.open(url, '_blank');
    }

}
