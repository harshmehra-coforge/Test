"""
/policy endpoint: Bind policy.
"""
from flask import Blueprint, request, jsonify

policy_bp = Blueprint('policy', __name__)

@policy_bp.route('/policy', methods=['POST'])
def bind_policy():
    """Bind a new policy based on quote and user info."""
    # TODO: Parse input, call policy_service, return result
    return jsonify({"policy_number": "TODO", "status": "Active"})
