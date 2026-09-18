// CateringApp Modern Custom Select Handler (Downward, Scrollable Y, Strictly Hidden on Init)
function initCustomScrollSelects() {
    $('.custom-select-scroll').each(function () {
        var $select = $(this);
        if ($select.data('custom-select-init')) return;
        $select.data('custom-select-init', true);

        // Safely hide original select visually without breaking form submission or validation
        $select.css({
            position: 'absolute',
            opacity: '0',
            width: '1px',
            height: '1px',
            padding: '0',
            margin: '-1px',
            overflow: 'hidden',
            clip: 'rect(0,0,0,0)',
            border: '0',
            pointerEvents: 'none'
        });

        var $wrapper = $('<div class="custom-select-wrapper"></div>');
        var isInsideInputGroup = $select.parent().hasClass('input-group');
        if (isInsideInputGroup) {
            $wrapper.addClass('input-group-custom-select');
        }

        var selectedOption = $select.find('option:selected');
        var selectedText = (selectedOption.length && selectedOption.text()) ? selectedOption.text() : $select.find('option:first').text();

        var $trigger = $('<div class="custom-select-trigger" tabindex="0"></div>')
            .html('<span class="text-truncate mr-2">' + selectedText + '</span><i class="fas fa-chevron-down text-muted" style="font-size: 0.8rem; flex-shrink: 0;"></i>');

        if (isInsideInputGroup) {
            $trigger.css({
                'border-top-left-radius': '0',
                'border-bottom-left-radius': '0',
                'border-left': '0',
                'height': '46px',
                'border-top-right-radius': '30px',
                'border-bottom-right-radius': '30px',
                'border-color': '#dee2e6'
            });
        }

        // Strictly hidden by default inline style as well to prevent any unstyled flash
        var $dropdown = $('<div class="custom-select-dropdown" style="display: none !important;"></div>');

        $select.find('option').each(function () {
            var $opt = $(this);
            var isSelected = $opt.is(':selected');
            var $item = $('<div class="custom-select-item"></div>')
                .text($opt.text())
                .attr('data-value', $opt.val());

            if (isSelected) {
                $item.addClass('selected');
            }

            $item.on('click', function (e) {
                e.stopPropagation();
                var val = $(this).attr('data-value');
                $select.val(val);
                $trigger.find('span').text($(this).text());
                $dropdown.find('.custom-select-item').removeClass('selected');
                $(this).addClass('selected');

                // Close dropdown immediately
                $dropdown.removeClass('show').attr('style', 'display: none !important;');
                $trigger.removeClass('active');

                // Fire native and jQuery change events
                $select.trigger('change');
                if (typeof $select.valid === 'function') {
                    $select.valid();
                }

                // If element has auto-submit or onchange
                var onchangeAttr = $select.attr('onchange');
                if (onchangeAttr && onchangeAttr.indexOf('submit') !== -1) {
                    $select.closest('form').submit();
                }
            });

            $dropdown.append($item);
        });

        $trigger.on('click', function (e) {
            e.stopPropagation();
            var isOpen = $dropdown.hasClass('show');

            // Close all other dropdowns
            $('.custom-select-dropdown').removeClass('show').attr('style', 'display: none !important;');
            $('.custom-select-trigger').removeClass('active');

            if (!isOpen) {
                $dropdown.addClass('show').attr('style', 'display: block !important;');
                $trigger.addClass('active');

                // Scroll to active item if present
                var $activeItem = $dropdown.find('.custom-select-item.selected');
                if ($activeItem.length) {
                    var itemOffset = $activeItem.position().top;
                    $dropdown.scrollTop($dropdown.scrollTop() + itemOffset - 50);
                }
            }
        });

        // Keyboard navigation (Enter / Space / Escape)
        $trigger.on('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ' || e.key === 'ArrowDown') {
                e.preventDefault();
                $trigger.trigger('click');
            } else if (e.key === 'Escape') {
                $dropdown.removeClass('show').attr('style', 'display: none !important;');
                $trigger.removeClass('active');
            }
        });

        $select.after($wrapper);
        $wrapper.append($trigger).append($dropdown);

        // Sync back if select value changed from external code
        $select.on('change.customSelectSync', function () {
            var currentVal = $select.val();
            $dropdown.find('.custom-select-item').each(function () {
                if ($(this).attr('data-value') == currentVal) {
                    $(this).addClass('selected');
                    $trigger.find('span').text($(this).text());
                } else {
                    $(this).removeClass('selected');
                }
            });
        });
    });
}

// Global listener to close open dropdowns on click outside
$(document).on('click', function () {
    $('.custom-select-dropdown').removeClass('show').attr('style', 'display: none !important;');
    $('.custom-select-trigger').removeClass('active');
});

// Prevent dropdown clicks from bubbling to document and closing immediately
$(document).on('click', '.custom-select-dropdown', function (e) {
    e.stopPropagation();
});

$(document).ready(function () {
    initCustomScrollSelects();
});
