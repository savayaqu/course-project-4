<?php

namespace App\Exceptions\Api;

use Illuminate\Http\Exceptions\HttpResponseException;


class NotFoundException extends HttpResponseException
{
    public function __construct($class = null)
    {
        $message = ucfirst(($class ? class_basename($class) . ' ' : '') . 'not found');
        parent::__construct(throw new ApiException($message, 404));
    }
}
