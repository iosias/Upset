function prepEverything() {
    $.SearchedAtLeastOnce = false;
    $.ItemCompleteTipShown = false;
    $.UrlPrefix = "http://ddragon.leagueoflegends.com/cdn/5.16.1/img/";
    $.Builds = {};
    $.Summoners = {};
    $('#summoner-get-button').click(function () { getSummoner($('#summoner-name-input').val()); });
    $('#build-name-input').keyup(function () {
        changeBuildName();
    });

    $('.download-overlay').click(function () {
        $('.download-overlay').fadeOut();
        $('body').css('overflow', '');
    });
    $('.download-popover').click(function () {
        return false;
    });
    $('#summoner-name-input').keypress(function (e) {
        if (e.which == 13) {
            $('#summoner-get-button').click();
            return false;
        }
    });
    $('#summoner-name-input').focus(function (e) {
        $('#error-results-div').hide();
        return false;
    });
}

function getSummoner(name) {
    if (name != "") {
        $("#small-header").show();
        $('.jumbo-hide').hide();
        $('#build-set-results').hide();
        $('#summoner-get-control').hide();
        $('#loading-results-div').fadeIn();
        $.getJSON("/Home/RecentBuildsJson", { SummonerName: name }).done(
            function (data) {
                $('#loading-results-div').hide();
                $('#summoner-get-control').slideDown();
                if (data != null) {
                    processSummoner(data);
                } else {
                    $('#error-text-span').text("No recent matches found for " + name + ".");
                    $('#error-results-div').show();
                }
            }
            ).fail(
            function (error) {
                $('#error-text-span').text("Oh no! Something went wrong.");
                $('#error-results-div').show();
                $('#loading-results-div').hide();
                $('#summoner-get-control').slideDown();
            }
            );
    }
    else {
        $('#error-text-span').text("I highly doubt your summoner name is just blank.");
        $('#error-results-div').show();
    }
}

function processSummoner(buildSets) {
    if ($.SearchedAtLeastOnce) {
        $('#build-set-results').empty();
    }
    $.SearchedAtLeastOnce = true;
    for (var bc = buildSets.length - 1; bc >= 0 ; bc--) {
        var build = buildSets[bc];
        createBuildDiv(build);
        $.Builds[build.Id] = build;
    }
    $('#build-set-results').slideDown();

}

function toggleBuild(div) {
    div.find(".build-set-body").slideToggle();
}

function createBuildDiv(build) {
    var buildDiv = $("<div></div>").addClass("build-set");
    if (build.FullBuild) { buildDiv.addClass('full-build'); }
    $('#build-set-results').append(buildDiv);

    var header = $("<div></div>").addClass("build-set-header");

    buildDiv.append(header);
    var champSection = $("<div></div>").addClass("inline-block champ-section");
    header.append(champSection);

    var champImageContainer = $("<div></div>").addClass("inline-block champ-image");
    champSection.append(champImageContainer);

    var champ = build.Champion;
    var champImg = $(' <img src="' + $.UrlPrefix + 'champion/' + champ.Image.Full + '" alt="' + champ.Name + '">');
    champImageContainer.append(champImg);

    var champInfoBlock = $("<div></div>").addClass("inline-block champ-info");
    champSection.append(champInfoBlock);
    var champNameBlock = $("<div></div>").addClass("champion-header").html(champ.Name);
    champInfoBlock.append(champNameBlock);
    var champTitleBlock = $("<div></div>").addClass("champion-title").html(champ.Title);
    //champInfoBlock.append(champTitleBlock);


    var fbSummary = $('<div></div>').addClass("full-build-preview");
    header.append(fbSummary);

    for (var ic = 0; ic < build.FinalBuild.Items.length; ic++) {
        var item = build.FinalBuild.Items[ic];

        var itemBlock = $('<div></div>').addClass("item-purchase-block");
        fbSummary.append(itemBlock);

        var itemImage = $(' <img src="' + $.UrlPrefix + 'item/' + item.Image.Full + '" alt="' + item.Name + '">');
        itemBlock.append(itemImage);
    }
    for (var ic = build.FinalBuild.Items.length; ic < 7; ic++) {
        var itemBlock = $('<div></div>').addClass("item-purchase-block");
        fbSummary.append(itemBlock);
        var itemImage = $(' <img src="http://i.imgur.com/1s9hLJE.png">');
        itemBlock.append(itemImage);
    }

    var whenBlock = $("<div></div>").addClass("date-detail-block inline-block right").html(build.TimeSince);
    header.append(whenBlock);

    var downloadDiv = $('<div></div>').addClass("inline-block right");
    header.append(downloadDiv);
    var downloadButton = $('<button class="btn btn-primary download-build-button none" type="button">Download</button>');
    downloadDiv.append(downloadButton);

    var holder = $('<div></div>').addClass("inline-block");
    var fullBuildDiv = $('<div>FULL BUILD</div>').addClass("build-indicator");
    holder.append(fullBuildDiv);
    header.append(holder);

    var buildBody = $("<div></div>").addClass("build-set-body none");
    buildDiv.append(buildBody);
    var fetchPromptContainer = $("<div></div>").addClass("fetch-prompt");
    buildBody.append(fetchPromptContainer);
    var loadingMessage = $("<div class='loading-message'><img src='http://i.imgur.com/U3ivgWI.gif' height='30' width='30' />  Getting match details.</div>");
    fetchPromptContainer.append(loadingMessage);
    header.click(function () {
        $('#games-found').slideUp();
        buildBody.slideToggle();
        fbSummary.fadeToggle();
        downloadButton.fadeToggle();
        if (!$.Builds[build.Id].MatchDataFetched) {
            getMatchData(build.Id, build.SummonerId, build.TotalDamageDealt, buildBody);
        }
    });

    downloadButton.click(function () {
        $.CurrentBuild = buildDiv.data('build');
        $.CurrentBuild.title = "Upset " + champ.Name;
        $('#build-name-input').val($.CurrentBuild.title);
        $('#build-download-link')[0].href = 'data:text/plain;charset=utf-8,' + encodeURIComponent(JSON.stringify($.CurrentBuild));
        $('#build-download-link')[0].download = champ.Key + "Upset.json";
        $('#download-champ-name-span').text(champ.Name);
        $('#champ-key-replace').text(champ.Key);
        $('#build-name-input').val($.CurrentBuild.title);
        $('.download-overlay').fadeIn();
        $('body').css('overflow', 'hidden');
        return false;
    });
}

function getMatchData(buildId, summonerId, damage, buildBody) {
    $.getJSON("/Home/GetMatchJson", { MatchId: buildId, SummonerId: summonerId, Damage: damage }).done(
    function (build) {
        if (build != null) {
            $.Builds[build.Id] = build;
            buildBody.empty();
            buildBody.parent().data('build', makeBuildObject(build));
            if (build.InitialPurchase) buildBody.append(BuildPurchaseSetDiv(build.InitialPurchase));
            if (build.RushItem) buildBody.append(BuildPurchaseSetDiv(build.RushItem));
            if (build.FinalBuild) buildBody.append(BuildPurchaseSetDiv(build.FinalBuild, true));
            if (build.Consumables) buildBody.append(BuildPurchaseSetDiv(build.Consumables));

            if (!build.FullBuild && !$.ItemCompleteTipShown) {
                buildBody.append($('<div class ="announcement tool-tip" id="item-complete-tip">Mouse over incomplete items in your final build to replace them with complete items!</div>'));
                $.ItemCompleteTipShown = true;
            }
        } else {
            $('#error-text-span').text("Oh no! Something went wrong.");
            $('#error-results-div').show();
            $('#loading-results-div').hide();
            $('#summoner-get-control').slideDown();
        }
    }
    ).fail(
    function (error) {
        $('#error-text-span').text("Oh no! Something went wrong.");
        $('#error-results-div').show();
        $('#loading-results-div').hide();
        $('#summoner-get-control').slideDown();
    }
    )
}

function makeBuildObject(build) {
    var buildObject = {
        title: "Upset Marmoset",
        type: "custom",
        map: "any",
        mode: "any",
        priority: true,
        sortrank: 0,
        blocks: []
    }
    buildObject.blocks.push(makeBuildBlock(build.InitialPurchase));
    if (build.RushItem) buildObject.blocks.push(makeBuildBlock(build.RushItem, true));
    buildObject.blocks.push(makeBuildBlock(build.FinalBuild));
    if(build.Consumables) buildObject.blocks.push(makeBuildBlock(build.Consumables, false, true));
    return buildObject;
}

function makeBuildBlock(purchaseSet, recMath, noPurchaseLimit) {
    noPurchaseLimit = noPurchaseLimit === true;
    recMath = recMath === true;
    var set = {
        "type": purchaseSet.Name,
        "recMath": recMath,
        "minSummonerLevel": -1,
        "maxSummonerLevel": -1,
        "showIfSummonerSpell": "",
        "hideIfSummonerSpell": "",
        "items": []
    };
    for (var i = 0; i < purchaseSet.Items.length; i++) {
        var item = purchaseSet.Items[i];
        var id = item.Id !== 2010 ? item.Id + "" : "2003";
        var placed = false;
        for (var j = 0; j < set.items.length; j++) {
            var setItem = set.items[j];
            if (setItem.id == id) {
                set.items[j].count++;
                placed = true;
                break;
            }
        }
        if (!placed) {
            set.items.push(
            {
                "id": id,
                "count": (noPurchaseLimit ? -1 : 1),
            });
        }
    }
    return set;
}

function BuildPurchaseSetDiv(purchaseSet, completions) {
    var hasCompletions = completions === true;
    var psDiv = $('<div></div>').addClass("purchase-group");
    var headerText = purchaseSet.Name;
    var psHeader = $('<div></div>').addClass("purchase-group-header").text(headerText);
    psDiv.append(psHeader);

    var psBody = $('<div></div>').addClass("purchase-group-body");
    psDiv.append(psBody);

    for (var i = 0; i < purchaseSet.Items.length; i++) {
        var item = purchaseSet.Items[i];

        var itemBlock = $('<div></div>').addClass("item-purchase-block");
        if (hasCompletions && $.PotentialUpgrades[item.Id]) {
            itemBlock.addClass("upgradable");
            itemBlock.data('index', i);
            var upgradeHolder = $('<div></div>').addClass("upgrades-block-container");
            itemBlock.append(upgradeHolder);
            var upgradeDiv = $('<div></div>').addClass("upgrades-block");
            upgradeHolder.append(upgradeDiv);
            var upgradeHeader = $('<div></div>').addClass("upgrade-header");
            upgradeDiv.append(upgradeHeader);
            var itemImage = $(' <img src="' + $.UrlPrefix + 'item/' + item.Image.Full + '" alt="' + item.Name + '">');
            upgradeHeader.append(itemImage);

            var upgradeTextSpan = $('<span>Select a completion</span>');
            upgradeHeader.append(upgradeTextSpan);

            var upgradeBody = $('<div></div>').addClass("upgrade-body");
            upgradeDiv.append(upgradeBody);
            for (var j = 0; j < $.PotentialUpgrades[item.Id].length; j++) {
                var upgrade = $.PotentialUpgrades[item.Id][j];
                buildUpgradeDiv(upgradeBody, upgrade, itemBlock, item)
            }
        }
        psBody.append(itemBlock);

        var itemImage = $(' <img src="' + $.UrlPrefix + 'item/' + item.Image.Full + '" alt="' + item.Name + '">');
        itemBlock.append(itemImage);
    }

    if (hasCompletions) {
        for (var j = purchaseSet.Items.length; j < 7; j++) {
            var itemBlock = $('<div></div>').addClass("item-purchase-block upgradable");
            itemBlock.data('index', -1);
            psBody.append(itemBlock);
            var itemImage = $(' <img src="http://i.imgur.com/1s9hLJE.png">');
            itemBlock.append(itemImage);

            var unbuiltHolder = $('<div></div>').addClass("upgrades-block-container");
            itemBlock.append(unbuiltHolder);
            var upgradeDiv = $('<div></div>').addClass("upgrades-block unbuilt-upgrade");
            unbuiltHolder.append(upgradeDiv);
            var upgradeHeader = $('<div></div>').addClass("upgrade-header");
            upgradeDiv.append(upgradeHeader);
            var upgradeSubheader = $('<div></div>').addClass("upgrade-subheader");
            upgradeDiv.append(upgradeSubheader);
            var searchBar = $('<input type="text" class="form-control" placeholder="Search">').addClass("completion-search");
            upgradeSubheader.append(searchBar);
            var upgradeTextSpan = $('<span>What were you going to build?</span>');
            upgradeHeader.append(upgradeTextSpan);

            var upgradeBody = $('<div></div>').addClass("upgrade-body");
            upgradeDiv.append(upgradeBody);
            for (var k = 0; k < $.CompleteItems.length; k++) {
                var completion = $.CompleteItems[k];
                buildUpgradeDiv(upgradeBody, completion, itemBlock, 0);
            }
            hookUpSearch(searchBar, upgradeBody.children());

        }
    }
    return psDiv;
}

function hookUpSearch(searchBar, searchElements) {
    searchBar.keyup(function () {
        var searchTerm = searchBar.val();
        if (searchTerm.trim() == '') {
            for (var i = 0; i < searchElements.length; i++) {
                var upgradeDiv = searchElements[i];
                $(upgradeDiv).show();
                return;
            }
        }
        var matches = {};
        var search = searchTerm.toUpperCase();
        for (var i = 0; i < $.CompleteItems.length; i++) {
            var completion = $.CompleteItems[i];
            if (completion.Name.toUpperCase().indexOf(search) != -1 ||
               completion.Name.toUpperCase().indexOf(search) != -1 ||
                completion.Name.toUpperCase().indexOf(search) != -1) {
                matches[completion.Id] = true;
            }
            else {
                matches[completion.Id] = false;
            }
        }
        for (var i = 0; i < searchElements.length; i++) {
            var upgradeDiv = searchElements[i];
            if (matches[$(upgradeDiv).data('item')]) {
                $(upgradeDiv).show();
            }
            else {
                $(upgradeDiv).hide();
            }
        }
    });
}


function buildUpgradeDiv(upgradeDiv, upgrade, itemBlock, item) {
    var upgradeOptionDiv = $('<div></div>').addClass("upgrade-option").data('item', upgrade.Id);
    upgradeDiv.append(upgradeOptionDiv);

    var itemImage = $(' <img src="' + $.UrlPrefix + 'item/' + upgrade.Image.Full + '" alt="' + upgrade.Name + '" title="' + upgrade.Name + '">');
    upgradeOptionDiv.append(itemImage);
    upgradeOptionDiv.click(function () {
        upgradeItem(itemBlock, item, upgrade, itemImage);
    });
}


function upgradeItem(oldDiv, oldItem, newItem, newImage) {
    $('#item-complete-tip').slideUp();
    var itemIndex = oldDiv.data('index');
    oldDiv.empty();
    oldDiv.append(newImage);
    oldDiv.removeClass('upgradable');
    var parent = oldDiv.parent();
    while (!parent.data('build')) {
        parent = parent.parent();
    }
    var build = parent.data('build');
    var blockIndex = -1;
    for (var i = 0; i < build.blocks.length; i++) {
        if (build.blocks[i].type == "Final Build") {
            blockIndex = i;
            break;
        }
    }
    if (blockIndex > -1) {
        if (itemIndex > -1) {
            build.blocks[blockIndex].items[itemIndex].id = newItem.Id + "";
        }
        else {
            build.blocks[blockIndex].items.push(
            {
                "id": newItem.Id + "",
                "count": 1,
            });
        }
    }
    if (parent.find('.upgradable').length == 0) {
        parent.addClass('full-build');
    }
    parent.data('build', build);
}

function changeBuildName()
{
    $.CurrentBuild.title = $('#build-name-input').val();
    $('#build-download-link')[0].href = 'data:text/plain;charset=utf-8,' + encodeURIComponent(JSON.stringify($.CurrentBuild));
}