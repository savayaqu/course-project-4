<?php

namespace App\Http\Requests\Api\Settings;

use App\Http\Requests\Api\ApiRequest;

class SettingsUpdateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'key' => 'required|string|in:' . implode(',', [
                'allowed_upload_mimes',
                'allowed_preview_sizes',
                'warning_limit_for_ban',
                'free_storage_limit',
                'upload_disable_percentage'
            ]),
            'value' => 'required|string',
        ];
    }
}
